using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;

namespace WebSocketTest.Http
{
    public class HttpResponse
    {
        public int StatusCode { get; set; } = 200; public string ContentType { get; set; } = "text/html; charset=utf-8"; public string Body { get; set; } = ""; public List<string> SetCookies { get; } = new(); public string RedirectUrl { get; set; }
        public static HttpResponse Html(string html, int status = 200) => new() { StatusCode = status, Body = html };
        public static HttpResponse Redirect(string url) => new() { StatusCode = 302, RedirectUrl = url };
        public void AddCookie(string name, string value, string path = "/", int maxAgeSec = 3600) { SetCookies.Add($"{name}={value}; Path={path}; Max-Age={maxAgeSec}; HttpOnly"); }
        public void ClearCookie(string name) { SetCookies.Add($"{name}=; Path=/; Max-Age=0"); }
        public byte[] ToBytes()
        {
            var sb = new StringBuilder();
            string statusText = StatusCode switch { 200 => "OK", 302 => "Found", 400 => "Bad Request", 401 => "Unauthorized", 403 => "Forbidden", 404 => "Not Found", 405 => "Method Not Allowed", _ => "OK" };
            sb.AppendLine($"HTTP/1.1 {StatusCode} {statusText}"); sb.AppendLine($"Content-Type: {ContentType}");
            if (!string.IsNullOrEmpty(RedirectUrl)) sb.AppendLine($"Location: {RedirectUrl}");
            foreach (var c in SetCookies) sb.AppendLine($"Set-Cookie: {c}");
            byte[] bodyBytes = Encoding.UTF8.GetBytes(Body); sb.AppendLine($"Content-Length: {bodyBytes.Length}"); sb.AppendLine("Connection: close"); sb.AppendLine();
            byte[] headerBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[headerBytes.Length + bodyBytes.Length]; Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length); Buffer.BlockCopy(bodyBytes, 0, result, headerBytes.Length, bodyBytes.Length);
            return result;
        }
    }
}