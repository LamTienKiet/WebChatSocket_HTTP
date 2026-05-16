using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Web;

using System.Text;
using System.Web;

namespace WebSocketTest.Http
{
    public class HttpRequest
    {
        public string Method { get; private set; }
        public string Path { get; private set; }
        public string RawPath { get; private set; }
        public Dictionary<string, string> Headers { get; private set; } = new();
        public Dictionary<string, string> Cookies { get; private set; } = new();
        public string Body { get; private set; } = "";
        public Dictionary<string, string> FormData { get; private set; } = new();
        public bool IsWebSocketRequest { get; private set; }

        public static HttpRequest Parse(string rawRequest)
        {
            var req = new HttpRequest();
            if (string.IsNullOrWhiteSpace(rawRequest)) return req;

            var lines = rawRequest.Replace("\r\n", "\n").Split('\n');
            if (lines.Length == 0) return req;

            // Request line: GET /path HTTP/1.1
            var parts = lines[0].Trim().Split(' ');
            if (parts.Length >= 2)
            {
                req.Method = parts[0].ToUpper();
                req.RawPath = parts[1];
                // Lấy path không query string
                req.Path = req.RawPath.Split('?')[0];
            }

            // Parse headers
            int i = 1;
            for (; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line == "") break;

                int colon = line.IndexOf(':');
                if (colon > 0)
                {
                    string key = line[..colon].Trim().ToLower();
                    string val = line[(colon + 1)..].Trim();
                    req.Headers[key] = val;
                }
            }

            // Parse cookies
            if (req.Headers.TryGetValue("cookie", out string cookieStr))
            {
                foreach (var part in cookieStr.Split(';'))
                {
                    var kv = part.Trim().Split('=', 2);
                    if (kv.Length == 2)
                        req.Cookies[kv[0].Trim()] = kv[1].Trim();
                }
            }

            // Body
            if (i + 1 < lines.Length)
            {
                req.Body = string.Join("\n", lines[(i + 1)..]).Trim();

                // Parse form data
                if (req.Headers.TryGetValue("content-type", out string ct) &&
                    ct.Contains("application/x-www-form-urlencoded"))
                {
                    req.FormData = ParseFormData(req.Body);
                }
            }

            // WebSocket check
            req.IsWebSocketRequest =
                req.Headers.TryGetValue("upgrade", out var up) &&
                up.ToLower() == "websocket";

            return req;
        }

        public static Dictionary<string, string> ParseFormData(string body)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var part in body.Split('&'))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2)
                    dict[Uri.UnescapeDataString(kv[0])] =
                        Uri.UnescapeDataString(kv[1].Replace('+', ' '));
            }
            return dict;
        }

        public string GetCookie(string name)
            => Cookies.TryGetValue(name, out var v) ? v : null;
    }
}

