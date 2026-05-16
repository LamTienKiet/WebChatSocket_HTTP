using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using WebSocketTest.Http;
using WebSocketTest.Models;
using WebSocketTest.Router;

namespace WebSocketTest.Http
{
    public class HttpServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _running;
        private readonly WebRouter _router;

        // WebSocket clients: clientId -> (TcpClient, NetworkStream, username)
        private readonly Dictionary<string, WsClient> _wsClients = new();
        private readonly object _wsLock = new();

        public HttpServer(int port, WebRouter router)
        {
            _port = port;
            _router = router;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _running = true;

            Console.WriteLine($"[HttpServer] Listening on http://localhost:{_port}/");
            Console.WriteLine($"[HttpServer] WebSocket: ws://localhost:{_port}/ws");

            Task.Run(AcceptLoop);
        }

        public void Stop()
        {
            _running = false;
            _listener.Stop();
        }

        private async Task AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                }
                catch when (!_running) { }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Accept] {ex.Message}");
                }
            }
        }

        private async Task HandleClient(TcpClient tcpClient)
        {
            try
            {
                using var stream = tcpClient.GetStream();
                stream.ReadTimeout = 5000;

                // Đọc request
                string rawRequest = await ReadHttpRequest(stream);
                if (string.IsNullOrEmpty(rawRequest)) return;

                var req = HttpRequest.Parse(rawRequest);

                // WebSocket Upgrade
                if (req.IsWebSocketRequest && req.Path == "/ws")
                {
                    await HandleWebSocketUpgrade(tcpClient, stream, req, rawRequest);
                    return;
                }

                // HTTP thông thường
                var resp = _router.Route(req);
                byte[] respBytes = resp.ToBytes();
                await stream.WriteAsync(respBytes);

                Console.WriteLine($"[HTTP] {req.Method} {req.Path} -> {resp.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] {ex.Message}");
            }
        }

        // ----------------------------------------------------------------
        // Đọc HTTP request từ stream
        // ----------------------------------------------------------------
        private static async Task<string> ReadHttpRequest(NetworkStream stream)
        {
            byte[] buffer = new byte[8192];
            var sb = new StringBuilder();

            do
            {
                int read = await stream.ReadAsync(buffer);
                if (read == 0) break;
                sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
            }
            while (stream.DataAvailable);

            return sb.ToString();
        }

        // ----------------------------------------------------------------
        // WebSocket Upgrade Handshake
        // ----------------------------------------------------------------
        private async Task HandleWebSocketUpgrade(
            TcpClient tcpClient,
            NetworkStream stream,
            HttpRequest req,
            string rawRequest)
        {
            // Lấy Sec-WebSocket-Key từ header raw (case sensitive)
            string wsKey = null;
            foreach (var line in rawRequest.Split("\r\n"))
            {
                if (line.StartsWith("Sec-WebSocket-Key:", StringComparison.OrdinalIgnoreCase))
                {
                    wsKey = line.Split(':', 2)[1].Trim();
                    break;
                }
            }

            if (wsKey == null)
            {
                tcpClient.Close();
                return;
            }

            // Tạo accept key
            string acceptKey = ComputeWsAcceptKey(wsKey);

            string handshake =
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n";

            await stream.WriteAsync(Encoding.UTF8.GetBytes(handshake));

            // Đọc username từ cookie
            string username = null;
            string token = req.GetCookie("auth_token");
            if (token != null)
                username = Data.TokenStore.Instance.GetUsername(token);

            if (username == null)
            {
                tcpClient.Close();
                return;
            }

            string clientId = Guid.NewGuid().ToString("N")[..8];
            var wsClient = new WsClient(tcpClient, stream, username, clientId);

            lock (_wsLock)
                _wsClients[clientId] = wsClient;

            Console.WriteLine($"[WS] {username} connected ({clientId})");

            await WsBroadcast($"[SYSTEM] {username} da tham gia chat");

            await WsHandleLoop(wsClient);
        }

        // ----------------------------------------------------------------
        // WebSocket Message Loop
        // ----------------------------------------------------------------
        private async Task WsHandleLoop(WsClient client)
        {
            try
            {
                while (client.TcpClient.Connected)
                {
                    byte[] frame = await WsReadFrame(client.Stream);
                    if (frame == null) break;

                    string msg = Encoding.UTF8.GetString(frame);
                    if (string.IsNullOrWhiteSpace(msg)) continue;

                    Console.WriteLine($"[WS] {client.Username}: {msg}");

                    string outMsg = $"[{client.Username}] {msg}";
                    await WsBroadcast(outMsg);
                }
            }
            catch { /* disconnect */ }

            lock (_wsLock)
                _wsClients.Remove(client.ClientId);

            await WsBroadcast($"[SYSTEM] {client.Username} da roi khoi chat");
            Console.WriteLine($"[WS] {client.Username} disconnected");
        }

        // ----------------------------------------------------------------
        // WebSocket Frame Reading (RFC 6455)
        // ----------------------------------------------------------------
        private static async Task<byte[]> WsReadFrame(NetworkStream stream)
        {
            try
            {
                byte[] header = new byte[2];
                if (!await ReadExact(stream, header)) return null;

                bool masked = (header[1] & 0x80) != 0;
                int opcode = header[0] & 0x0F;

                if (opcode == 8) return null; // Close frame

                long length = header[1] & 0x7F;

                if (length == 126)
                {
                    byte[] ext = new byte[2];
                    await ReadExact(stream, ext);
                    length = (ext[0] << 8) | ext[1];
                }
                else if (length == 127)
                {
                    byte[] ext = new byte[8];
                    await ReadExact(stream, ext);
                    length = BitConverter.ToInt64(ext.Reverse().ToArray());
                }

                byte[] mask = new byte[4];
                if (masked) await ReadExact(stream, mask);

                byte[] payload = new byte[length];
                await ReadExact(stream, payload);

                if (masked)
                    for (int i = 0; i < payload.Length; i++)
                        payload[i] ^= mask[i % 4];

                return payload;
            }
            catch { return null; }
        }

        private static async Task<bool> ReadExact(NetworkStream stream, byte[] buf)
        {
            int total = 0;
            while (total < buf.Length)
            {
                int read = await stream.ReadAsync(buf, total, buf.Length - total);
                if (read == 0) return false;
                total += read;
            }
            return true;
        }

        // ----------------------------------------------------------------
        // WebSocket Frame Writing (RFC 6455, no mask from server)
        // ----------------------------------------------------------------
        private static byte[] WsWrapFrame(string msg)
        {
            byte[] payload = Encoding.UTF8.GetBytes(msg);
            byte[] frame;

            if (payload.Length <= 125)
            {
                frame = new byte[2 + payload.Length];
                frame[0] = 0x81; // FIN + text
                frame[1] = (byte)payload.Length;
                Buffer.BlockCopy(payload, 0, frame, 2, payload.Length);
            }
            else if (payload.Length <= 65535)
            {
                frame = new byte[4 + payload.Length];
                frame[0] = 0x81;
                frame[1] = 126;
                frame[2] = (byte)(payload.Length >> 8);
                frame[3] = (byte)(payload.Length & 0xFF);
                Buffer.BlockCopy(payload, 0, frame, 4, payload.Length);
            }
            else
            {
                frame = new byte[10 + payload.Length];
                frame[0] = 0x81;
                frame[1] = 127;
                long len = payload.Length;
                for (int i = 7; i >= 0; i--)
                {
                    frame[2 + (7 - i)] = (byte)(len >> (i * 8));
                }
                Buffer.BlockCopy(payload, 0, frame, 10, payload.Length);
            }

            return frame;
        }

        private async Task WsBroadcast(string message)
        {
            byte[] frame = WsWrapFrame(message);

            List<WsClient> clients;
            lock (_wsLock)
                clients = _wsClients.Values.ToList();

            foreach (var c in clients)
            {
                try
                {
                    await c.Stream.WriteAsync(frame);
                }
                catch { /* ignore */ }
            }
        }

        // ----------------------------------------------------------------
        // Compute WebSocket Accept Key
        // ----------------------------------------------------------------
        private static string ComputeWsAcceptKey(string key)
        {
            const string magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            using var sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(key + magic));
            return Convert.ToBase64String(hash);
        }

        private class WsClient
        {
            public TcpClient TcpClient { get; }
            public NetworkStream Stream { get; }
            public string Username { get; }
            public string ClientId { get; }

            public WsClient(TcpClient tc, NetworkStream ns, string username, string id)
            {
                TcpClient = tc;
                Stream = ns;
                Username = username;
                ClientId = id;
            }
        }
    }
}
