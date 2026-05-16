🚀 WebSocket Chat Application (C#)
📌 Overview

This project is a lightweight real-time chat web application built using C#, HttpListener, and WebSocket. It demonstrates core backend concepts along with the practical application of software design patterns such as Singleton and Builder.

The system allows users to log in, join chat rooms, and communicate in real time with other users through a browser-based interface.

🧠 Design Patterns Used
🔹 1. Singleton Pattern

The Singleton pattern ensures that only one instance of a class exists during the application lifecycle.

It is applied in:

UserJsonReader → loads user data from JSON only once
TokenStore → manages authentication tokens globally

Benefits:

Reduces redundant file I/O
Ensures consistent session management
Improves performance
🔹 2. Builder Pattern (HtmlBuilder)

The HtmlBuilder class is used to dynamically generate HTML pages instead of using static .html files.

Usage:

new HtmlBuilder()
    .SetTitle("Chat")
    .SetTheme("dark")
    .SetBody(content)
    .Build();

Features:

Supports dark/light themes
Clean separation between UI and logic
Reusable UI construction
🔐 Authentication & Session
Users are authenticated via a JSON file (user.json)
After login:
A token is generated using TokenStore
Token is stored in cookies
Each request validates user via:
TokenStore.Instance.GetUsername(token);

Security features:

Token expiration
Single login per user
Cookie-based session handling
💬 Features
Real-time chat using WebSocket
Multiple chat rooms
Online / Offline status
Message persistence (file-based)
Auto-scroll and responsive UI
Login / Logout system
Session management via cookies
----------------------------
🏗️ Project Structure
WebSocketTest/
│
├── Builder/
│   └── HtmlBuilder.cs          # Builder Pattern (UI generation)
│
├── Data/
│   ├── UserJsonReader.cs       # Singleton (read JSON users)
│   └── TokenStore.cs           # Singleton (manage tokens)
│
├── Router/
│   └── WebRouter.cs            # Handle HTTP routes + UI rendering
│
├── Http/
│   └── HttpResponse.cs         # HTTP response + cookie handling
│
├── Models/
│   └── ChatRoom.cs             # Chat room logic
│
├── ChatServerSingleton.cs      # WebSocket server (Singleton)
│
└── Data/user.json              # User database
-----------------------
⚙️ How to Run
1. Clone project
git clone <your-repo-link>
2. Open in Visual Studio
3. Run server
dotnet run
4. Open browser
http://localhost:PORT

🔑 Default Account
Username: Admin
Password: 123
🧪 Demo Flow
Go to /login
Enter credentials
Redirect to /chat
Join a room
Send messages in real time
