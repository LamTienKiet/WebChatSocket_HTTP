using WebSocketTest.Builder;
using WebSocketTest.Data;
using WebSocketTest.Http;
using WebSocketTest.Models;

namespace WebSocketTest.Router
{
    public class WebRouter
    {
        // Thông tin sinh viên
        private const string StudentId = "22302141";
        private const string StudentName = "Lâm Tiến Kiệt";
        private const int PcNumber = 15;

        private readonly Dictionary<string, ChatRoom> _rooms;
        private readonly string _dataDir;

        public WebRouter(Dictionary<string, ChatRoom> rooms, string dataDir)
        {
            _rooms = rooms;
            _dataDir = dataDir;
        }

        public HttpResponse Route(HttpRequest req)
        {
            string method = req.Method;
            string path = req.Path;

            // Normalize path
            if (path != "/" && path.EndsWith("/"))
                path = path.TrimEnd('/');

            Console.WriteLine($"[Router] {method} {path}");

            return (method, path) switch
            {
                ("GET", "/") => HandleIndex(req),
                ("GET", "/login") => HandleLoginGet(req),
                ("POST", "/login") => HandleLoginPost(req),
                ("GET", "/logout") => HandleLogout(req),
                ("GET", "/chat") => HandleChatList(req),
                ("GET", _) when path.StartsWith("/chat/") => HandleChatRoom(req, path[6..]),
                ("POST", _) when path.StartsWith("/chat/") => HandleChatPost(req, path[6..]),
                _ => HttpResponse.Html(BuildNotFound(), 404)
            };
        }

        // ----------------------------------------------------------------
        // GET /
        // ----------------------------------------------------------------
        private HttpResponse HandleIndex(HttpRequest req)
        {
            string username = GetLoggedInUser(req);

            string loginSection = username != null
                ? $@"<div class='alert alert-success'>Xin chao, <strong>{username}</strong>! Ban da dang nhap.</div>
                     <div style='display:flex;gap:1rem;flex-wrap:wrap;'>
                       <a href='/chat' class='btn btn-primary'>Vao Phong Chat</a>
                       <a href='/logout' class='btn btn-outline'>Dang Xuat</a>
                     </div>"
                : $@"<p style='color:var(--text-muted);margin-bottom:1.5rem;font-size:0.95rem;'>
                       Dang nhap de tham gia phong chat truc tuyen.
                     </p>
                     <a href='/login' class='btn btn-primary'>Dang Nhap Ngay</a>";

            string body = $@"
<nav class='nav'>
  <a href='/' class='nav-brand'>&gt; ChatServer_</a>
  {(username != null
    ? $"<a href='/logout' class='nav-link'>Dang Xuat</a>"
    : "<a href='/login' class='nav-link'>Dang Nhap</a>")}
</nav>
<div class='container'>
  <div class='profile-card fade-in'>
    <div class='profile-avatar'>{StudentName.Split(' ').Last()[0]}</div>
    <h1 class='page-title' style='text-align:center;margin-bottom:0.25rem;'>{StudentName}</h1>
    <p style='text-align:center;color:var(--text-muted);font-family:Fira Code,monospace;font-size:0.85rem;margin-bottom:2rem;'>
      Thong tin sinh vien
    </p>

    <div class='profile-field fade-in fade-in-1'>
      <span class='profile-label'>Ma So Sinh Vien</span>
      <code class='profile-value'>{StudentId}</code>
    </div>
    <div class='profile-field fade-in fade-in-2'>
      <span class='profile-label'>Ho va Ten</span>
      <span class='profile-value'>{StudentName}</span>
    </div>
    <div class='profile-field fade-in fade-in-3'>
      <span class='profile-label'>So May</span>
      <span class='profile-value'>PC #{PcNumber}</span>
    </div>

    <div class='divider'></div>
    <div class='fade-in fade-in-4'>
      {loginSection}
    </div>
  </div>
</div>";

            return HttpResponse.Html(
                new HtmlBuilder()
                    .SetTitle("Thong Tin Sinh Vien")
                    .SetTheme("dark")
                    .SetBody(body)
                    .Build()
            );
        }

        // ----------------------------------------------------------------
        // GET /login
        // ----------------------------------------------------------------
        private HttpResponse HandleLoginGet(HttpRequest req)
        {
            // Nếu đã đăng nhập → redirect
            if (GetLoggedInUser(req) != null)
                return HttpResponse.Redirect("/chat");

            string body = BuildLoginPage("", false);
            return HttpResponse.Html(
                new HtmlBuilder()
                    .SetTitle("Dang Nhap")
                    .SetTheme("dark")
                    .SetBody(body)
                    .Build()
            );
        }

        // ----------------------------------------------------------------
        // POST /login
        // ----------------------------------------------------------------
        private HttpResponse HandleLoginPost(HttpRequest req)
        {
            string username = req.FormData.TryGetValue("username", out var u) ? u : "";
            string password = req.FormData.TryGetValue("password", out var p) ? p : "";

            if (AuthService.Validate(username, password))
            {
                // Tạo token
                string token = TokenStore.Instance.CreateToken(username);

                var resp = HttpResponse.Redirect("/chat");
                resp.AddCookie("auth_token", token, "/", 3600);
                resp.AddCookie("username", username, "/", 3600);
                return resp;
            }

            // Đăng nhập thất bại
            string body = BuildLoginPage(username, true);
            return HttpResponse.Html(
                new HtmlBuilder()
                    .SetTitle("Dang Nhap - That Bai")
                    .SetTheme("dark")
                    .SetBody(body)
                    .Build(),
                401
            );
        }

        // ----------------------------------------------------------------
        // GET /logout
        // ----------------------------------------------------------------
        private HttpResponse HandleLogout(HttpRequest req)
        {
            string token = req.GetCookie("auth_token");
            if (token != null)
                TokenStore.Instance.Remove(token);

            var resp = HttpResponse.Redirect("/login");
            resp.ClearCookie("auth_token");
            resp.ClearCookie("username");
            return resp;
        }

        // ----------------------------------------------------------------
        // GET /chat
        // ----------------------------------------------------------------
        private HttpResponse HandleChatList(HttpRequest req)
        {
            string username = GetLoggedInUser(req);
            if (username == null)
                return HttpResponse.Redirect("/login");

            // Cập nhật trạng thái phòng: xóa lịch sử nếu offline
            foreach (var room in _rooms.Values)
                room.ClearIfOffline();

            var sb = new System.Text.StringBuilder();

            sb.Append($@"
<nav class='nav'>
  <a href='/' class='nav-brand'>&gt; ChatServer_</a>
  <div style='display:flex;align-items:center;gap:1rem;'>
    <span style='font-size:0.85rem;color:var(--text-muted);font-family:Fira Code,monospace;'>
      {username}
    </span>
    <a href='/logout' class='nav-link'>Dang Xuat</a>
  </div>
</nav>
<div class='container'>
  <h1 class='page-title fade-in'>Phong Tro Chuyen</h1>
  <p class='page-sub fade-in'>Chon mot phong de bat dau tro chuyen.</p>
  <div class='room-list'>");

            int delay = 1;
            foreach (var room in _rooms.Values.OrderBy(r => r.Id))
            {
                string statusBadge = room.IsOnline
                    ? "<span class='badge badge-online'>online</span>"
                    : "<span class='badge badge-offline'>offline</span>";

                string offlineClass = room.IsOnline ? "" : " offline";
                string lastActive = room.LastActivity == DateTime.MinValue
                    ? "Chua co hoat dong"
                    : $"Lan cuoi: {room.LastActivity:HH:mm dd/MM}";

                int msgCount = room.Messages.Count;

                sb.Append($@"
    <a href='/chat/{room.Id}' class='room-item{offlineClass} fade-in fade-in-{Math.Min(delay++, 4)}'>
      <div class='room-info'>
        <span class='room-name'>{room.Name}</span>
        <span class='room-meta'>{lastActive} &bull; {msgCount} tin nhan</span>
      </div>
      <div style='display:flex;align-items:center;gap:0.75rem;'>
        {statusBadge}
        <svg width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='2'>
          <polyline points='9 18 15 12 9 6'/>
        </svg>
      </div>
    </a>");
            }

            sb.Append("</div></div>");

            return HttpResponse.Html(
                new HtmlBuilder()
                    .SetTitle("Phong Chat")
                    .SetTheme("dark")
                    .SetBody(sb.ToString())
                    .Build()
            );
        }

        // ----------------------------------------------------------------
        // GET /chat/:id
        // ----------------------------------------------------------------
        private HttpResponse HandleChatRoom(HttpRequest req, string roomId)
        {
            string username = GetLoggedInUser(req);
            if (username == null)
                return HttpResponse.Redirect("/login");

            if (!_rooms.TryGetValue(roomId, out var room))
                return HttpResponse.Html(BuildNotFound(), 404);

            // Xóa lịch sử nếu offline
            room.ClearIfOffline();

            var msgSb = new System.Text.StringBuilder();

            if (room.Messages.Count == 0)
            {
                msgSb.Append(@"
  <div style='text-align:center;padding:3rem;color:var(--text-muted);'>
    <div style='font-size:2.5rem;margin-bottom:0.75rem;'>[ ]</div>
    <p style='font-family:Fira Code,monospace;font-size:0.85rem;'>Chua co tin nhan nao. Hay bat dau cuoc tro chuyen!</p>
  </div>");
            }
            else
            {
                foreach (var msg in room.Messages.OrderBy(m => m.Time))
                {
                    bool isOwn = string.Equals(msg.Username, username,
                        StringComparison.OrdinalIgnoreCase);

                    string css = isOwn ? "own" : "other";

                    msgSb.Append($@"
  <div class='message {css}'>
    <div class='message-bubble'>{EscapeHtml(msg.Content)}</div>
    <div class='message-meta'>{msg.Username} &bull; {msg.Time:HH:mm:ss}</div>
  </div>");
                }
            }

            string statusBadge = room.IsOnline
                ? "<span class='badge badge-online'>online</span>"
                : "<span class='badge badge-offline'>offline</span>";

            string dotHtml = room.IsOnline
                ? "<span class='online-dot'></span>"
                : "";

            string body = $@"
<nav class='nav'>
  <a href='/' class='nav-brand'>&gt; ChatServer_</a>
  <div style='display:flex;align-items:center;gap:1rem;'>
    <a href='/chat' class='nav-link'>Danh Sach Phong</a>
    <a href='/logout' class='nav-link'>Dang Xuat</a>
  </div>
</nav>
<div class='chat-layout'>
  <div class='chat-header'>
    <div class='chat-room-name'>
      {dotHtml}
      {room.Name}
      {statusBadge}
    </div>
    <span style='font-size:0.8rem;color:var(--text-muted);font-family:Fira Code,monospace;'>
      {room.Messages.Count} tin nhan
    </span>
  </div>

  <div class='chat-messages' id='messages'>
    {msgSb}
  </div>

  <div class='chat-input-area'>
    <form method='POST' action='/chat/{roomId}' class='chat-form' id='chatForm'>
      <textarea
        name='message'
        class='chat-input'
        placeholder='Nhap tin nhan...'
        id='msgInput'
        rows='1'
        required
      ></textarea>
      <button type='submit' class='btn btn-primary'>Gui</button>
    </form>
  </div>
</div>

<script>
  // Auto-scroll
  var msgs = document.getElementById('messages');
  if (msgs) msgs.scrollTop = msgs.scrollHeight;

  // Enter to submit (Shift+Enter for newline)
  var inp = document.getElementById('msgInput');
  if (inp) {{
    inp.addEventListener('keydown', function(e) {{
      if (e.key === 'Enter' && !e.shiftKey) {{
        e.preventDefault();
        document.getElementById('chatForm').submit();
      }}
    }});
  }}
</script>";

            return HttpResponse.Html(
                new HtmlBuilder()
                    .SetTitle(room.Name + " - Chat")
                    .SetTheme("dark")
                    .SetBody(body)
                    .Build()
            );
        }

        // ----------------------------------------------------------------
        // POST /chat/:id
        // ----------------------------------------------------------------
        private HttpResponse HandleChatPost(HttpRequest req, string roomId)
        {
            string username = GetLoggedInUser(req);
            if (username == null)
                return HttpResponse.Redirect("/login");

            if (!_rooms.TryGetValue(roomId, out var room))
                return HttpResponse.Html(BuildNotFound(), 404);

            string message = req.FormData.TryGetValue("message", out var m) ? m.Trim() : "";

            if (!string.IsNullOrWhiteSpace(message))
            {
                room.AddMessage(username, message);
                room.SaveToFile(_dataDir);
                Console.WriteLine($"[Chat/{roomId}] {username}: {message}");
            }

            return HttpResponse.Redirect($"/chat/{roomId}");
        }

        // ----------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------
        private string GetLoggedInUser(HttpRequest req)
        {
            string token = req.GetCookie("auth_token");
            return TokenStore.Instance.GetUsername(token);
        }

        private static string EscapeHtml(string s) =>
            s.Replace("&", "&amp;")
             .Replace("<", "&lt;")
             .Replace(">", "&gt;")
             .Replace("\"", "&quot;");

        private string BuildLoginPage(string username, bool failed)
        {
            string errorHtml = failed
                ? @"<div class='alert alert-error fade-in'>
                     Ten dang nhap hoac mat khau khong chinh xac. Vui long thu lai.
                   </div>"
                : "";

            string body = $@"
<nav class='nav'>
  <a href='/' class='nav-brand'>&gt; ChatServer_</a>
  <a href='/' class='nav-link'>Trang Chu</a>
</nav>
<div class='container'>
  <div class='profile-card fade-in' style='max-width:400px;'>
    <h1 class='page-title' style='text-align:center;'>Dang Nhap</h1>
    <p style='text-align:center;color:var(--text-muted);font-family:Fira Code,monospace;
              font-size:0.82rem;margin-bottom:2rem;'>
      Nhap thong tin tai khoan de tiep tuc.
    </p>

    {errorHtml}

    <form method='POST' action='/login'>
      <div class='form-group fade-in fade-in-1'>
        <label class='form-label'>Ten Dang Nhap</label>
        <input
          type='text'
          name='username'
          class='form-input'
          value='{EscapeHtml(username)}'
          placeholder='admin'
          autocomplete='username'
          required
        />
      </div>

      <div class='form-group fade-in fade-in-2'>
        <label class='form-label'>Mat Khau</label>
        <input
          type='password'
          name='password'
          class='form-input'
          placeholder='••••••••'
          autocomplete='current-password'
          required
        />
      </div>

      <div class='fade-in fade-in-3' style='margin-top:1.5rem;'>
        <button type='submit' class='btn btn-primary' style='width:100%;justify-content:center;'>
          Dang Nhap
        </button>
      </div>
    </form>

    <div class='divider'></div>
    <p style='font-size:0.78rem;color:var(--text-muted);text-align:center;font-family:Fira Code,monospace;'>
      Tai khoan mac dinh: Admin / 123
    </p>
  </div>
</div>";

            return body;
        }

        private static string BuildNotFound() => @"
<nav class='nav'>
  <a href='/' class='nav-brand'>&gt; ChatServer_</a>
</nav>
<div class='container' style='text-align:center;padding-top:5rem;'>
  <div style='font-family:Fira Code,monospace;font-size:5rem;color:var(--accent);margin-bottom:1rem;'>404</div>
  <h2 style='font-size:1.4rem;margin-bottom:0.75rem;'>Trang Khong Tim Thay</h2>
  <p style='color:var(--text-muted);margin-bottom:2rem;'>Duong dan ban truy cap khong ton tai.</p>
  <a href='/' class='btn btn-outline'>Ve Trang Chu</a>
</div>";
    }
}
