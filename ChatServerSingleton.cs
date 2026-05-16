
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketTest.Data;

namespace WebSocketTest
{
    // SINGLETON PATTERN
    public sealed class ChatServerSingleton
    {
        private static readonly Lazy<ChatServerSingleton> _instance =
            new(() => new ChatServerSingleton());

        public static ChatServerSingleton Instance => _instance.Value;

        private readonly HttpListener _listener;

        // Lưu client theo ID
        private readonly ConcurrentDictionary<string, ClientConnection> _clients = new();

        // username -> clientId (single login)
        private readonly ConcurrentDictionary<string, string> _userSessions = new();

        private bool _isRunning;

        // Constructor private
        private ChatServerSingleton()
        {
            _listener = new HttpListener();
        }

        // START SERVER
        public void Start(int port)
        {
            if (_isRunning) return;

            _listener.Prefixes.Add($"http://localhost:{port}/");
            _listener.Start();

            _isRunning = true;

            Console.WriteLine($"🚀 Server running at ws://localhost:{port}/");

            Task.Run(AcceptLoop);
        }

        // ACCEPT CLIENT
        private async Task AcceptLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    // Không phải WebSocket
                    if (!context.Request.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        continue;
                    }

                    // LOGIN từ user.json
                    string username = context.Request.QueryString["user"];
                    string password = context.Request.QueryString["pass"];

                    if (!AuthService.Validate(username, password))
                    {
                        context.Response.StatusCode = 401;
                        context.Response.Close();

                        Console.WriteLine($"❌ Login fail: {username}");
                        continue;
                    }

                    // Single login -> đá tài khoản cũ
                    await HandleDuplicateLogin(username);

                    // Upgrade WebSocket
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    var socket = wsContext.WebSocket;

                    string clientId = Guid.NewGuid().ToString()[..8];

                    var client = new ClientConnection(socket, clientId)
                    {
                        Name = username
                    };

                    _clients[clientId] = client;
                    _userSessions[username] = clientId;

                    Console.WriteLine($"👤 {username} connected (ID: {clientId})");

                    _ = HandleClient(client);
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Console.WriteLine($"❌ Accept error: {ex.Message}");
                    }
                }
            }
        }

        // HANDLE LOGIN TRÙNG
        private async Task HandleDuplicateLogin(string username)
        {
            if (_userSessions.TryGetValue(username, out string oldId))
            {
                if (_clients.TryGetValue(oldId, out var oldClient))
                {
                    await oldClient.CloseAsync("login elsewhere");
                    _clients.TryRemove(oldId, out _);

                    Console.WriteLine($"⚠️ {username} bị đăng xuất do login mới");
                }
            }
        }

        // HANDLE CLIENT
        private async Task HandleClient(ClientConnection client)
        {
            await Broadcast($"[SYSTEM] {client.Name} joined");

            byte[] buffer = new byte[1024];

            try
            {
                while (client.Socket.State == WebSocketState.Open)
                {
                    var result = await client.Socket.ReceiveAsync(
                        buffer,
                        CancellationToken.None
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    string message = Encoding.UTF8
                        .GetString(buffer, 0, result.Count)
                        .Trim();

                    if (string.IsNullOrWhiteSpace(message))
                        continue;

                    Console.WriteLine($"💬 {client.Name}: {message}");

                    await Broadcast($"[{client.Name}] {message}");
                }
            }
            catch
            {
                // Ignore socket errors
            }

            await DisconnectClient(client);
        }

        // DISCONNECT CLIENT
        private async Task DisconnectClient(ClientConnection client)
        {
            _clients.TryRemove(client.ClientId, out _);
            _userSessions.TryRemove(client.Name, out _);

            await Broadcast($"[SYSTEM] {client.Name} left");

            await client.CloseAsync();

            Console.WriteLine($"👋 {client.Name} disconnected");
        }

        // BROADCAST MESSAGE
        private async Task Broadcast(string message)
        {
            foreach (var client in _clients.Values)
            {
                if (client.Socket.State == WebSocketState.Open)
                {
                    await client.SendAsync(message);
                }
            }
        }

        // STOP SERVER
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            Console.WriteLine("🛑 Server stopped");
        }
    }
}


