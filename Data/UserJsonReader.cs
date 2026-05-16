using System.Text.Json;
using System.Text.Json.Serialization;
using WebSocketTest.Models;
namespace WebSocketTest.Data
{
    // SINGLETON PATTERN: chỉ đọc file JSON một lần
    public sealed class UserJsonReader
    {
        private static readonly Lazy<UserJsonReader> _instance =
            new(() => new UserJsonReader());

        public static UserJsonReader Instance => _instance.Value;

        private List<User> _users = new();

        private UserJsonReader()
        {
            Load();
        }

        private void Load()
        {
            try
            {
                string path = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data", "user.json"
                );

                if (!File.Exists(path))
                {
                    Console.WriteLine("[UserJsonReader] user.json not found!");
                    return;
                }

                string json = File.ReadAllText(path);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _users = JsonSerializer.Deserialize<List<User>>(json, options)
                         ?? new List<User>();

                Console.WriteLine($"[UserJsonReader] Loaded {_users.Count} users.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserJsonReader] Error: {ex.Message}");
                _users = new List<User>();
            }
        }

        public bool Validate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            return _users.Any(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password
            );
        }

        public IReadOnlyList<User> GetAll() => _users.AsReadOnly();
    }
}
