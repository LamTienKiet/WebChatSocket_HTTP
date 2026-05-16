using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketTest.Models
{
    public class ChatMessage
    {
        public DateTime Time { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }

        public ChatMessage(string username, string content)
        {
            Time = DateTime.Now;
            Username = username;
            Content = content;
        }

        public string ToLine() =>
            $"{Time:yyyy-MM-dd HH:mm:ss}|{Username}|{Content}";

        public static ChatMessage FromLine(string line)
        {
            var parts = line.Split('|', 3);
            if (parts.Length < 3) return null;
            return new ChatMessage(parts[1], parts[2])
            {
                Time = DateTime.TryParse(parts[0], out var dt) ? dt : DateTime.Now
            };
        }
    }
}

