using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using System.Collections.Concurrent;

namespace WebSocketTest.Data
{
    // Quản lý session token → username
    public sealed class TokenStore
    {
        private static readonly Lazy<TokenStore> _instance =
            new(() => new TokenStore());

        public static TokenStore Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, TokenEntry> _tokens = new();

        private TokenStore() { }

        public string CreateToken(string username)
        {
            // Xóa token cũ của user này
            var old = _tokens
                .Where(kv => kv.Value.Username == username)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var k in old)
                _tokens.TryRemove(k, out _);

            string token = Guid.NewGuid().ToString("N");

            _tokens[token] = new TokenEntry
            {
                Username = username,
                CreatedAt = DateTime.Now
            };

            return token;
        }

        public string GetUsername(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            if (_tokens.TryGetValue(token, out var entry))
            {
                
                if ((DateTime.Now - entry.CreatedAt).TotalMinutes < 3)
                    return entry.Username;

                _tokens.TryRemove(token, out _);
            }

            return null;
        }

        public void Remove(string token)
        {
            _tokens.TryRemove(token, out _);
        }

        private class TokenEntry
        {
            public string Username { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}

