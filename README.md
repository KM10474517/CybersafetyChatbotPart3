# Cybersafety Chatbot

A Windows desktop chatbot that teaches everyday cybersecurity. Ask it about phishing, passwords, privacy and more, take a quiz, and keep a to-do list of security tasks, all through a simple chat window.

Built with **C#, WPF and .NET 8**, with **MySQL** for storing tasks.

## Features

- **Cybersecurity Q&A:** keyword matching with varied answers on phishing, passwords, privacy, safe browsing, malware, two-factor authentication and VPNs
- **Sentiment detection:** notices when the user sounds worried or frustrated and adjusts its reply
- **Memory:** remembers your name and favourite topic, and handles follow-up questions such as "tell me more"
- **Cybersecurity quiz:** 11 questions with scoring and feedback
- **Task manager:** add, view, complete and delete tasks using natural phrases like "remind me to update my password"; tasks are stored in MySQL
- **Activity log:** a record of what you and the chatbot have done in the session
- **Quick-action buttons:** View Tasks, Add Task, Start Quiz, Activity Log and Help
- **Voice greeting:** plays a WAV greeting on startup if `Resources/ChatbotGreeting.wav` is next to the executable

## Tech Stack

| Area | Technology |
|------|------------|
| Language | C# |
| UI | WPF (XAML), .NET 8 |
| Database | MySQL (`MySql.Data` package) |

## Getting Started

**Requirements:** Windows, the .NET 8 SDK, and a MySQL server running on `localhost`.

1. Clone the repo:
   ```
   git clone https://github.com/KM10474517/CybersafetyChatbotPart3.git
   ```
2. Open `CybersafetyChatbotPart3.sln` in Visual Studio.
3. In `DatabaseHelper.cs`, update the connection string to match your MySQL user and password (it defaults to user `root` with no password). The `cyberchatbot` database and `tasks` table are created automatically on first run.
4. Press **F5** to build and run.

## Example Commands

- `what is phishing`
- `tell me more`
- `add task update my passwords`
- `view tasks`
- `complete task 1`
- `start quiz`
- `show activity log`
- `help`

## Project Structure

| File | Purpose |
|------|---------|
| `ChatEngine.cs` | Chat logic: keywords, sentiment, memory and command handling |
| `QuizEngine.cs` | Quiz questions, scoring and flow |
| `DatabaseHelper.cs` | MySQL setup and task storage |
| `ActivityLog.cs` | Session activity log |
| `TaskItem.cs` | Task data model |
| `Voicegreeting.cs` | Startup voice greeting |
| `MainWindow.xaml(.cs)` | Chat window UI |

## Author

Kamohelo Mopeli
