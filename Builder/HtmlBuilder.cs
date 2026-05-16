using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;

namespace WebSocketTest.Builder
{
    // BUILDER PATTERN: xây dựng giao diện HTML
    public class HtmlBuilder
    {
        private string _title = "Chat App";
        private string _bodyContent = "";
        private string _extraHead = "";
        private string _theme = "dark"; // dark | light

        // --- Builder Methods ---
        public HtmlBuilder SetTitle(string title)
        {
            _title = title;
            return this;
        }

        public HtmlBuilder SetTheme(string theme)
        {
            _theme = theme;
            return this;
        }

        public HtmlBuilder SetBody(string content)
        {
            _bodyContent = content;
            return this;
        }

        public HtmlBuilder AddHead(string headContent)
        {
            _extraHead += headContent;
            return this;
        }

        // --- Build ---
        public string Build()
        {
            string cssVars = _theme == "dark" ? DarkTheme() : LightTheme();

            return $@"<!DOCTYPE html>
<html lang='vi'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<title>{_title}</title>
<link rel='preconnect' href='https://fonts.googleapis.com'>
<link href='https://fonts.googleapis.com/css2?family=Fira+Code:wght@400;500;600&family=Space+Grotesk:wght@300;400;600;700&display=swap' rel='stylesheet'>
<style>
{cssVars}
*, *::before, *::after {{ box-sizing: border-box; margin: 0; padding: 0; }}

body {{
  font-family: 'Space Grotesk', sans-serif;
  background: var(--bg);
  color: var(--text);
  min-height: 100vh;
  line-height: 1.6;
}}

/* SCROLLBAR */
::-webkit-scrollbar {{ width: 6px; }}
::-webkit-scrollbar-track {{ background: var(--bg-2); }}
::-webkit-scrollbar-thumb {{ background: var(--accent); border-radius: 3px; }}

/* NAV */
.nav {{
  background: var(--bg-2);
  border-bottom: 1px solid var(--border);
  padding: 0.75rem 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: sticky;
  top: 0;
  z-index: 100;
  backdrop-filter: blur(10px);
}}

.nav-brand {{
  font-family: 'Fira Code', monospace;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--accent);
  text-decoration: none;
  letter-spacing: 0.05em;
}}

.nav-link {{
  color: var(--text-muted);
  text-decoration: none;
  font-size: 0.9rem;
  padding: 0.4rem 0.9rem;
  border: 1px solid var(--border);
  border-radius: 6px;
  transition: all 0.2s;
  font-family: 'Fira Code', monospace;
}}

.nav-link:hover {{
  color: var(--accent);
  border-color: var(--accent);
  background: var(--accent-dim);
}}

/* CONTAINER */
.container {{
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem 1.5rem;
}}

/* CARD */
.card {{
  background: var(--bg-2);
  border: 1px solid var(--border);
  border-radius: 12px;
  padding: 2rem;
  margin-bottom: 1.5rem;
}}

/* FORM */
.form-group {{ margin-bottom: 1.2rem; }}

.form-label {{
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--text-muted);
  margin-bottom: 0.5rem;
  font-family: 'Fira Code', monospace;
}}

.form-input {{
  width: 100%;
  padding: 0.75rem 1rem;
  background: var(--bg-3);
  border: 1px solid var(--border);
  border-radius: 8px;
  color: var(--text);
  font-size: 1rem;
  font-family: 'Space Grotesk', sans-serif;
  transition: border-color 0.2s, box-shadow 0.2s;
  outline: none;
}}

.form-input:focus {{
  border-color: var(--accent);
  box-shadow: 0 0 0 3px var(--accent-dim);
}}

/* BUTTON */
.btn {{
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  border: none;
  cursor: pointer;
  font-size: 0.95rem;
  font-weight: 600;
  font-family: 'Space Grotesk', sans-serif;
  transition: all 0.2s;
  text-decoration: none;
  letter-spacing: 0.02em;
}}

.btn-primary {{
  background: var(--accent);
  color: var(--bg);
}}

.btn-primary:hover {{
  background: var(--accent-bright);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px var(--accent-dim);
}}

.btn-outline {{
  background: transparent;
  color: var(--accent);
  border: 1px solid var(--accent);
}}

.btn-outline:hover {{
  background: var(--accent-dim);
}}

.btn-danger {{
  background: var(--danger);
  color: white;
}}

.btn-sm {{
  padding: 0.4rem 0.9rem;
  font-size: 0.85rem;
}}

/* ALERT */
.alert {{
  padding: 0.85rem 1.2rem;
  border-radius: 8px;
  font-size: 0.9rem;
  margin-bottom: 1rem;
  border: 1px solid;
}}

.alert-error {{
  background: var(--danger-dim);
  border-color: var(--danger);
  color: var(--danger-text);
}}

.alert-success {{
  background: var(--success-dim);
  border-color: var(--success);
  color: var(--success-text);
}}

/* BADGE */
.badge {{
  display: inline-block;
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.72rem;
  font-weight: 700;
  font-family: 'Fira Code', monospace;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}}

.badge-online {{
  background: var(--success-dim);
  color: var(--success);
  border: 1px solid var(--success);
}}

.badge-offline {{
  background: var(--bg-3);
  color: var(--text-muted);
  border: 1px solid var(--border);
}}

/* ROOM LIST */
.room-list {{ display: flex; flex-direction: column; gap: 0.75rem; }}

.room-item {{
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  background: var(--bg-3);
  border: 1px solid var(--border);
  border-radius: 10px;
  text-decoration: none;
  color: var(--text);
  transition: all 0.2s;
  cursor: pointer;
}}

.room-item:hover {{
  border-color: var(--accent);
  background: var(--accent-dim);
  transform: translateX(4px);
}}

.room-item.offline {{ opacity: 0.55; }}

.room-info {{ display: flex; flex-direction: column; gap: 0.2rem; }}

.room-name {{
  font-weight: 600;
  font-size: 0.95rem;
}}

.room-meta {{
  font-size: 0.78rem;
  color: var(--text-muted);
  font-family: 'Fira Code', monospace;
}}

/* CHAT PAGE */
.chat-layout {{
  display: flex;
  flex-direction: column;
  height: calc(100vh - 60px);
}}

.chat-header {{
  padding: 1rem 1.5rem;
  background: var(--bg-2);
  border-bottom: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: space-between;
}}

.chat-room-name {{
  font-weight: 700;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}}

.online-dot {{
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--success);
  display: inline-block;
  box-shadow: 0 0 6px var(--success);
  animation: pulse 2s infinite;
}}

@keyframes pulse {{
  0%, 100% {{ opacity: 1; }}
  50% {{ opacity: 0.4; }}
}}

.chat-messages {{
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}}

.message {{
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
  max-width: 80%;
}}

.message.own {{
  align-self: flex-end;
  align-items: flex-end;
}}

.message.other {{
  align-self: flex-start;
}}

.message.system {{
  align-self: center;
  align-items: center;
  max-width: 100%;
}}

.message-bubble {{
  padding: 0.65rem 1rem;
  border-radius: 12px;
  font-size: 0.95rem;
  line-height: 1.5;
  word-break: break-word;
}}

.message.own .message-bubble {{
  background: var(--accent);
  color: var(--bg);
  border-bottom-right-radius: 4px;
}}

.message.other .message-bubble {{
  background: var(--bg-3);
  border: 1px solid var(--border);
  border-bottom-left-radius: 4px;
}}

.message.system .message-bubble {{
  background: var(--bg-3);
  border: 1px dashed var(--border);
  font-size: 0.8rem;
  color: var(--text-muted);
  font-family: 'Fira Code', monospace;
  padding: 0.4rem 0.9rem;
}}

.message-meta {{
  font-size: 0.72rem;
  color: var(--text-muted);
  font-family: 'Fira Code', monospace;
  padding: 0 0.25rem;
}}

.chat-input-area {{
  padding: 1rem 1.5rem;
  background: var(--bg-2);
  border-top: 1px solid var(--border);
}}

.chat-form {{
  display: flex;
  gap: 0.75rem;
  align-items: flex-end;
}}

.chat-input {{
  flex: 1;
  padding: 0.75rem 1rem;
  background: var(--bg-3);
  border: 1px solid var(--border);
  border-radius: 10px;
  color: var(--text);
  font-size: 0.95rem;
  font-family: 'Space Grotesk', sans-serif;
  outline: none;
  resize: none;
  min-height: 44px;
  max-height: 120px;
  transition: border-color 0.2s;
}}

.chat-input:focus {{
  border-color: var(--accent);
}}

/* PROFILE CARD */
.profile-card {{
  background: var(--bg-2);
  border: 1px solid var(--border);
  border-radius: 16px;
  padding: 2.5rem 2rem;
  max-width: 480px;
  margin: 3rem auto;
}}

.profile-avatar {{
  width: 80px;
  height: 80px;
  background: var(--accent-dim);
  border: 2px solid var(--accent);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: 700;
  color: var(--accent);
  font-family: 'Fira Code', monospace;
  margin: 0 auto 1.5rem;
}}

.profile-field {{
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--border);
}}

.profile-field:last-child {{ border-bottom: none; }}

.profile-label {{
  font-size: 0.8rem;
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--text-muted);
  font-family: 'Fira Code', monospace;
}}

.profile-value {{
  font-weight: 600;
  font-size: 0.95rem;
  color: var(--text);
}}

/* TITLE */
h1, h2, h3 {{
  font-weight: 700;
  letter-spacing: -0.02em;
}}

.page-title {{
  font-size: 1.8rem;
  margin-bottom: 0.5rem;
}}

.page-sub {{
  color: var(--text-muted);
  font-size: 0.9rem;
  margin-bottom: 2rem;
  font-family: 'Fira Code', monospace;
}}

/* CODE */
code {{
  font-family: 'Fira Code', monospace;
  font-size: 0.85em;
  background: var(--bg-3);
  padding: 0.15em 0.4em;
  border-radius: 4px;
  color: var(--accent);
}}

/* DIVIDER */
.divider {{
  height: 1px;
  background: var(--border);
  margin: 1.5rem 0;
}}

/* FADE IN ANIMATION */
@keyframes fadeInUp {{
  from {{ opacity: 0; transform: translateY(16px); }}
  to   {{ opacity: 1; transform: translateY(0); }}
}}

.fade-in {{ animation: fadeInUp 0.4s ease forwards; }}

.fade-in-1 {{ animation-delay: 0.05s; opacity: 0; }}
.fade-in-2 {{ animation-delay: 0.12s; opacity: 0; }}
.fade-in-3 {{ animation-delay: 0.2s; opacity: 0; }}
.fade-in-4 {{ animation-delay: 0.28s; opacity: 0; }}

{_extraHead}
</style>
</head>
<body>
{_bodyContent}
</body>
</html>";
        }

        private static string DarkTheme() => @"
:root {
  --bg:           #0d1117;
  --bg-2:         #161b22;
  --bg-3:         #21262d;
  --border:       #30363d;
  --text:         #e6edf3;
  --text-muted:   #7d8590;
  --accent:       #58a6ff;
  --accent-bright:#79c0ff;
  --accent-dim:   rgba(88,166,255,0.1);
  --danger:       #f85149;
  --danger-dim:   rgba(248,81,73,0.1);
  --danger-text:  #ffa198;
  --success:      #3fb950;
  --success-dim:  rgba(63,185,80,0.1);
  --success-text: #7ee787;
}";

        private static string LightTheme() => @"
:root {
  --bg:           #f6f8fa;
  --bg-2:         #ffffff;
  --bg-3:         #f0f2f5;
  --border:       #d0d7de;
  --text:         #1f2328;
  --text-muted:   #656d76;
  --accent:       #0969da;
  --accent-bright:#0550ae;
  --accent-dim:   rgba(9,105,218,0.08);
  --danger:       #d1242f;
  --danger-dim:   rgba(209,36,47,0.08);
  --danger-text:  #a40e26;
  --success:      #1a7f37;
  --success-dim:  rgba(26,127,55,0.08);
  --success-text: #116329;
}";
    }
}

