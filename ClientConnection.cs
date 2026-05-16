using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest
{
    public class ClientConnection
    {
        // WebSocket của client
        public WebSocket Socket { get; }

        // ID nội bộ (GUID rút gọn)
        public string ClientId { get; }

        //  Username (từ login)
        public string Name { get; set; }

        //  Constructor
        public ClientConnection(WebSocket socket, string id)
        {
            Socket = socket;
            ClientId = id[..8]; // rút gọn cho đẹp
        }

        //  Gửi message tới client
        public async Task SendAsync(string message)
        {
            if (Socket.State != WebSocketState.Open)
                return;

            byte[] data = Encoding.UTF8.GetBytes(message);

            await Socket.SendAsync(
                new ArraySegment<byte>(data),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        //  Đóng kết nối
        public async Task CloseAsync(string reason = "bye")
        {
            if (Socket.State == WebSocketState.Open)
            {
                await Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    reason,
                    CancellationToken.None
                );
            }
        }
    }
}