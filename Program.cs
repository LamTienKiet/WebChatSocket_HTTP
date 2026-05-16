using System.Text;
using WebSocketTest;
using WebSocketTest.Http;
using WebSocketTest.Models;
using WebSocketTest.Router;

namespace WebSocketTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("=========================================");
            Console.WriteLine("  CHAT WEB SERVER - C# Socket Edition   ");
            Console.WriteLine("  Singleton + Builder Design Patterns    ");
            Console.WriteLine("=========================================");

            // Data directory
            string dataDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data", "rooms"
            );
            Directory.CreateDirectory(dataDir);

            // Khởi tạo các phòng chat
            var rooms = new Dictionary<string, ChatRoom>
            {
                ["1"] = new ChatRoom("1", "Phong Chung"),
                ["2"] = new ChatRoom("2", "Phong Ky Thuat"),
                //["3"] = new ChatRoom("3", "Phong Giai Tri"),
                //["4"] = new ChatRoom("4", "Phong Hoc Tap"),
            };

            // Tải lịch sử từ file
            foreach (var room in rooms.Values)
                room.LoadFromFile(dataDir);

            // Khởi động HTTP Server (raw Socket)
            var router = new WebRouter(rooms, dataDir);
            var server = new HttpServer(8080, router);
            server.Start();

            Console.WriteLine();
            Console.WriteLine("  HTTP : http://localhost:8080/");
            Console.WriteLine("  WS   : ws://localhost:8080/ws");
            Console.WriteLine("  Nhan Q + Enter de dung server");
            Console.WriteLine("=========================================");

            // Giữ app chạy
            while (Console.ReadLine()?.Trim().ToUpper() != "Q") { }

            server.Stop();
            Console.WriteLine("Server da dung.");
        }
    }
}
