namespace WebSocketTest.Models
{
    public class ChatRoom
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime LastActivity { get; set; }
        public List<ChatMessage> Messages { get; private set; } = new();

        // Online nếu có hoạt động trong 3 phút
        public bool IsOnline => (DateTime.Now - LastActivity).TotalMinutes <= 3;

        public ChatRoom(string id, string name)
        {
            Id = id;
            Name = name;
            LastActivity = DateTime.MinValue;
        }

        public void AddMessage(string username, string content)
        {
            Messages.Add(new ChatMessage(username, content));
            LastActivity = DateTime.Now;
        }

        // Xóa lịch sử nếu offline
        public void ClearIfOffline()
        {
            if (!IsOnline)
                Messages.Clear();
        }

        // Lưu lịch sử vào file .txt
        public void SaveToFile(string dataDir)
        {
            Directory.CreateDirectory(dataDir);
            string path = Path.Combine(dataDir, $"room_{Id}.txt");
            File.WriteAllLines(path, Messages.Select(m => m.ToLine()));
        }

        // Tải lịch sử từ file .txt
        public void LoadFromFile(string dataDir)
        {
            string path = Path.Combine(dataDir, $"room_{Id}.txt");
            if (!File.Exists(path)) return;

            Messages = File.ReadAllLines(path)
                .Select(ChatMessage.FromLine)
                .Where(m => m != null)
                .ToList();

            if (Messages.Count > 0)
                LastActivity = Messages.Last().Time;
        }
    }
}
