using System;
using System.Collections.Generic;

namespace CybersafetyChatbotPart3
{
    // Main chatbot engine for Part 3
    // Includes everything from Part 2 plus tasks, quiz, NLP, and activity log
    public class ChatEngine
    {
        // ── Memory ───────────────────────────────────────────────────────────
        public string? UserName { get; private set; }
        public string? FavouriteTopic { get; private set; }
        private string? _lastTopic = null;

        // ── Part 3 features ──────────────────────────────────────────────────
        private DatabaseHelper _database = new DatabaseHelper();
        private QuizEngine _quiz = new QuizEngine();
        private ActivityLog _activityLog = new ActivityLog();

        // Tracks whether we are waiting for the user to confirm a reminder
        private bool _waitingForReminder = false;
        private string _pendingTaskTitle = "";
        private string _pendingTaskDescription = "";

        // Random number generator for random responses
        private Random _random = new Random();

        // ── Random response lists (from Part 2) ──────────────────────────────
        private List<string> _phishingResponses = new List<string>
        {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
            "Always check the sender's email address carefully. Fake emails often use addresses that look almost right but have small differences.",
            "Never click links in unexpected emails. Go directly to the website by typing the address in your browser instead.",
            "Legitimate companies will never ask for your password via email. If an email asks for it, it is a scam.",
            "Watch out for urgent language in emails like 'Act now' or 'Your account will be closed'. This is a common phishing tactic."
        };

        private List<string> _passwordResponses = new List<string>
        {
            "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
            "A strong password should be at least 12 characters long and include letters, numbers, and symbols.",
            "Never reuse the same password across multiple sites. If one site is hacked, all your accounts become vulnerable.",
            "Consider using a password manager like Bitwarden or 1Password to store your passwords securely.",
            "Avoid obvious passwords like your name, birthday, or the word 'password'. These are the first ones hackers try."
        };

        private List<string> _privacyResponses = new List<string>
        {
            "Limit the personal information you share on social media. Scammers can use it to target you.",
            "Review the permissions of apps on your phone regularly. Disable camera or microphone access for apps that do not need it.",
            "Use a privacy-focused browser like Firefox or Brave to reduce how much data is collected about you.",
            "Clear your browser cookies and cache regularly to reduce tracking.",
            "Opt out of data sharing wherever possible when signing up for new services."
        };

        private List<string> _browsingResponses = new List<string>
        {
            "Always check for 'https://' in the address bar before entering any personal information on a website.",
            "Avoid using public Wi-Fi for banking or logging into sensitive accounts.",
            "Keep your browser updated to protect against the latest security vulnerabilities.",
            "Use an ad blocker to reduce your exposure to malicious advertisements.",
            "Be careful about which browser extensions you install. Only use ones from trusted developers."
        };

        // ── Sentiment keywords (from Part 2) ─────────────────────────────────
        private List<string> _worriedKeywords = new List<string>
        {
            "worried", "scared", "afraid", "anxious", "nervous", "fear", "terrified", "concerned"
        };

        private List<string> _frustratedKeywords = new List<string>
        {
            "frustrated", "annoyed", "angry", "upset", "confused", "lost", "stuck", "dont understand", "don't understand"
        };

        // ── Follow-up keywords (from Part 2) ─────────────────────────────────
        private List<string> _followUpKeywords = new List<string>
        {
            "tell me more", "another tip", "explain more", "more info", "give me more", "what else", "continue", "go on"
        };

        // ── Constructor - initialise the database on startup ─────────────────
        public ChatEngine()
        {
            _database.InitialiseDatabase();
        }

        // ── Set the user's name ──────────────────────────────────────────────
        public void SetUserName(string name)
        {
            UserName = name.Trim();
            _activityLog.AddEntry("User session started for: " + UserName);
        }

        // ── Main response method ─────────────────────────────────────────────
        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "I didn't receive any input. Please type a question or topic.";

            string lower = input.Trim().ToLower();

            // ── Handle quiz answers if quiz is active ─────────────────────────
            if (_quiz.IsActive && !_quiz.IsFinished())
            {
                return HandleQuizAnswer(input);
            }

            // ── Handle reminder confirmation ──────────────────────────────────
            if (_waitingForReminder)
            {
                return HandleReminderResponse(lower, input);
            }

            // ── Step 1: Check for sentiment ───────────────────────────────────
            string? sentimentResponse = DetectSentiment(lower);
            if (sentimentResponse != null)
                return sentimentResponse;

            // ── Step 2: Check for follow-up questions ─────────────────────────
            if (IsFollowUp(lower) && _lastTopic != null)
                return GetFollowUpResponse();

            // ── Step 3: Activity log commands ─────────────────────────────────
            if (lower.Contains("show activity log") || lower.Contains("what have you done") || lower.Contains("activity log"))
            {
                _activityLog.AddEntry("User viewed activity log");
                return _activityLog.GetLog();
            }

            // ── Step 4: Quiz commands ─────────────────────────────────────────
            if (lower.Contains("start quiz") || lower.Contains("play quiz") || lower.Contains("take quiz") || lower.Contains("quiz"))
            {
                return StartQuiz();
            }

            // ── Step 5: Task commands (NLP keyword detection) ──────────────────
            if (lower.Contains("add task") || lower.Contains("create task") || lower.Contains("new task") ||
                lower.Contains("remind me to") || lower.Contains("add a reminder") || lower.Contains("set a reminder"))
            {
                return HandleAddTask(lower, input);
            }

            if (lower.Contains("view tasks") || lower.Contains("show tasks") || lower.Contains("my tasks") ||
                lower.Contains("list tasks") || lower.Contains("what tasks"))
            {
                return HandleViewTasks();
            }

            if (lower.Contains("complete task") || lower.Contains("mark task") || lower.Contains("finished task") ||
                lower.Contains("done task"))
            {
                return "To complete a task, type: 'complete task [number]'\nFor example: 'complete task 1'\n\nType 'view tasks' to see your task numbers.";
            }

            if (lower.Contains("delete task") || lower.Contains("remove task"))
            {
                return HandleDeleteTask(lower);
            }

            // Check for completing a specific task by number
            if (lower.StartsWith("complete task ") || lower.StartsWith("finish task "))
            {
                return HandleCompleteTask(lower);
            }

            // ── Step 6: Help ──────────────────────────────────────────────────
            if (lower.Contains("help"))
                return GetHelpMessage();

            // ── Step 7: Memory recall ─────────────────────────────────────────
            if (lower.Contains("what do you remember") || lower.Contains("what do you know about me"))
                return GetMemoryRecall();

            // ── Step 8: Cybersecurity keyword matching (from Part 2) ───────────
            if (lower.Contains("phishing") || lower.Contains("scam") || lower.Contains("fake email"))
            {
                _lastTopic = "phishing";
                StoreFavouriteTopic("phishing");
                _activityLog.AddEntry("User asked about phishing");
                return GetRandomResponse(_phishingResponses);
            }

            if (lower.Contains("password"))
            {
                _lastTopic = "password";
                StoreFavouriteTopic("password");
                _activityLog.AddEntry("User asked about passwords");
                return GetRandomResponse(_passwordResponses);
            }

            if (lower.Contains("privacy") || lower.Contains("personal data") || lower.Contains("private"))
            {
                _lastTopic = "privacy";
                StoreFavouriteTopic("privacy");
                _activityLog.AddEntry("User asked about privacy");
                return GetRandomResponse(_privacyResponses);
            }

            if (lower.Contains("browsing") || lower.Contains("browse") || lower.Contains("internet") || lower.Contains("website"))
            {
                _lastTopic = "browsing";
                StoreFavouriteTopic("browsing");
                _activityLog.AddEntry("User asked about safe browsing");
                return GetRandomResponse(_browsingResponses);
            }

            if (lower.Contains("malware") || lower.Contains("virus") || lower.Contains("ransomware"))
            {
                _lastTopic = "malware";
                StoreFavouriteTopic("malware");
                _activityLog.AddEntry("User asked about malware");
                return "Malware is malicious software designed to harm your device, " + UserName + ".\n" +
                       "• Always install a reputable antivirus and keep it updated.\n" +
                       "• Never download software from untrusted sources.\n" +
                       "• Back up your files regularly in case of a ransomware attack.\n" +
                       "Type 'tell me more' for additional tips.";
            }

            if (lower.Contains("2fa") || lower.Contains("two factor") || lower.Contains("two-factor") || lower.Contains("mfa"))
            {
                _lastTopic = "2fa";
                StoreFavouriteTopic("2fa");
                _activityLog.AddEntry("User asked about 2FA");
                return "Two-Factor Authentication adds an extra security layer, " + UserName + "!\n" +
                       "• Even if someone steals your password they still cannot log in.\n" +
                       "• Use an authenticator app like Google Authenticator or Authy.\n" +
                       "• Enable 2FA on your email, banking and social media accounts first.\n" +
                       "Type 'tell me more' for additional tips.";
            }

            if (lower.Contains("vpn"))
            {
                _lastTopic = "vpn";
                _activityLog.AddEntry("User asked about VPN");
                return "A VPN encrypts your internet traffic and hides your IP address, " + UserName + ".\n" +
                       "It is especially useful when using public Wi-Fi networks.\n" +
                       "Recommended options: ProtonVPN, Mullvad, or NordVPN.";
            }

            if (lower.Contains("breach") || lower.Contains("hacked") || lower.Contains("leaked"))
            {
                _lastTopic = "breach";
                _activityLog.AddEntry("User asked about data breaches");
                return "If your data has been breached, act quickly, " + UserName + "!\n" +
                       "1. Change your passwords immediately.\n" +
                       "2. Enable two-factor authentication on all accounts.\n" +
                       "3. Monitor your bank statements for unusual activity.\n" +
                       "4. Visit haveibeenpwned.com to check if your email was exposed.";
            }

            // ── General conversation ──────────────────────────────────────────
            if (lower.Contains("how are you"))
                return "I am doing great, " + UserName + "! Always alert and ready to help you stay safe online. 😊";

            if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
                return "Hello, " + UserName + "! How can I help you stay safe today?";

            if (lower.Contains("thank"))
                return "You are welcome, " + UserName + "! Staying informed is the best defence against cyber threats. 😊";

            if (lower.Contains("purpose") || lower.Contains("what can you do"))
                return GetHelpMessage();

            // ── Default response ──────────────────────────────────────────────
            return "I'm not sure I understand that, " + UserName + ". Could you try rephrasing?\n" +
                   "Type 'help' to see all available topics and commands.";
        }

        // ── Quiz methods ─────────────────────────────────────────────────────

        private string StartQuiz()
        {
            _quiz.StartQuiz();
            _activityLog.AddEntry("Quiz started");

            QuizQuestion? first = _quiz.GetCurrentQuestion();
            if (first == null) return "No questions available.";

            string response = "🎮 Cybersecurity Quiz Started! " + _quiz.TotalQuestions + " questions.\n";
            response += "Type A, B, C, or D to answer.\n\n";
            response += FormatQuestion(first, 1);
            return response;
        }

        private string HandleQuizAnswer(string input)
        {
            string feedback = _quiz.SubmitAnswer(input.Trim());

            if (_quiz.IsFinished())
            {
                string finalScore = _quiz.GetFinalScore();
                _activityLog.AddEntry("Quiz completed - Score: " + _quiz.Score + "/" + _quiz.TotalQuestions);
                return feedback + "\n\n" + finalScore;
            }

            // Get the next question
            QuizQuestion? next = _quiz.GetCurrentQuestion();
            if (next == null) return feedback;

            int questionNumber = _quiz.TotalQuestions - (_quiz.TotalQuestions - (_questions_answered() + 1)) + 1;
            return feedback + "\n\n" + FormatQuestion(next, questionNumber);
        }

        // Helper to count questions answered
        private int _questions_answered()
        {
            return _quiz.TotalQuestions - (int)(_quiz.TotalQuestions - _quiz.Score);
        }

        private string FormatQuestion(QuizQuestion q, int number)
        {
            string result = "Question " + number + ": " + q.Question + "\n\n";
            foreach (string option in q.Options)
            {
                result += option + "\n";
            }
            return result;
        }

        // ── Task methods ─────────────────────────────────────────────────────

        private string HandleAddTask(string lower, string originalInput)
        {
            // NLP: extract the task from various phrasings
            string taskTitle = "";
            string taskDescription = "";

            if (lower.Contains("remind me to"))
            {
                // e.g. "Remind me to update my password"
                int idx = lower.IndexOf("remind me to") + "remind me to".Length;
                taskTitle = originalInput.Substring(idx).Trim();
                taskDescription = "Reminder: " + taskTitle;
            }
            else if (lower.Contains("add task") || lower.Contains("create task") || lower.Contains("new task"))
            {
                // e.g. "Add task - Review privacy settings"
                string[] separators = { "-", ":", "–" };
                string remaining = originalInput;

                foreach (string sep in separators)
                {
                    if (remaining.Contains(sep))
                    {
                        taskTitle = remaining.Substring(remaining.IndexOf(sep) + 1).Trim();
                        break;
                    }
                }

                if (taskTitle == "")
                    taskTitle = "New cybersecurity task";

                taskDescription = GetTaskDescription(taskTitle.ToLower());
            }
            else if (lower.Contains("set a reminder") || lower.Contains("add a reminder"))
            {
                // e.g. "Set a reminder to check privacy settings"
                string[] keywords = { "to ", "for " };
                taskTitle = originalInput;
                foreach (string kw in keywords)
                {
                    if (lower.Contains(kw))
                    {
                        int idx = lower.IndexOf(kw) + kw.Length;
                        taskTitle = originalInput.Substring(idx).Trim();
                        break;
                    }
                }
                taskDescription = "Reminder: " + taskTitle;
            }

            // Store for reminder confirmation
            _pendingTaskTitle = taskTitle;
            _pendingTaskDescription = taskDescription;
            _waitingForReminder = true;

            return "Task added with the description \"" + taskDescription + "\"\nWould you like a reminder? If yes, type when (e.g. 'in 3 days', 'tomorrow'). If no, type 'no'.";
        }

        private string GetTaskDescription(string lower)
        {
            // Generate a relevant description based on the task keywords
            if (lower.Contains("password")) return "Update your passwords to ensure they are strong and unique.";
            if (lower.Contains("2fa") || lower.Contains("two factor")) return "Set up two-factor authentication to secure your accounts.";
            if (lower.Contains("privacy")) return "Review account privacy settings to ensure your data is protected.";
            if (lower.Contains("antivirus") || lower.Contains("virus")) return "Install or update your antivirus software.";
            if (lower.Contains("update") || lower.Contains("patch")) return "Update your software and operating system to the latest version.";
            if (lower.Contains("backup")) return "Back up your important files to prevent data loss.";
            return "Complete this cybersecurity task to stay safe online.";
        }

        private string HandleReminderResponse(string lower, string originalInput)
        {
            _waitingForReminder = false;
            string? reminder = null;

            if (lower != "no" && lower != "n")
            {
                reminder = originalInput.Trim();
                _database.AddTask(_pendingTaskTitle, _pendingTaskDescription, reminder);
                _activityLog.AddEntry("Task added: '" + _pendingTaskTitle + "' with reminder: " + reminder);
                return "Got it! I'll remind you " + reminder + ".\nTask '" + _pendingTaskTitle + "' has been saved. 📝";
            }
            else
            {
                _database.AddTask(_pendingTaskTitle, _pendingTaskDescription, null);
                _activityLog.AddEntry("Task added: '" + _pendingTaskTitle + "' (no reminder)");
                return "Task '" + _pendingTaskTitle + "' has been saved without a reminder. 📝\nType 'view tasks' to see all your tasks.";
            }
        }

        private string HandleViewTasks()
        {
            List<TaskItem> tasks = _database.GetAllTasks();
            _activityLog.AddEntry("User viewed task list");

            if (tasks.Count == 0)
                return "You have no tasks yet, " + UserName + ".\nType 'add task - [task name]' to add one!";

            string result = "📋 Your Cybersecurity Tasks:\n\n";
            int count = 1;

            foreach (TaskItem task in tasks)
            {
                string status = task.IsCompleted ? "✅" : "⏳";
                result += count + ". " + status + " " + task.Title + "\n";
                result += "   📝 " + task.Description + "\n";

                if (!string.IsNullOrEmpty(task.Reminder))
                    result += "   ⏰ Reminder: " + task.Reminder + "\n";

                result += "\n";
                count++;
            }

            result += "Type 'complete task [number]' or 'delete task [number]' to manage tasks.";
            return result;
        }

        private string HandleCompleteTask(string lower)
        {
            try
            {
                // Extract the task number from the input
                string[] parts = lower.Split(' ');
                int taskNumber = int.Parse(parts[parts.Length - 1]);

                List<TaskItem> tasks = _database.GetAllTasks();

                if (taskNumber < 1 || taskNumber > tasks.Count)
                    return "Task number " + taskNumber + " doesn't exist. Type 'view tasks' to see your tasks.";

                TaskItem task = tasks[taskNumber - 1];
                bool success = _database.CompleteTask(task.Id);

                if (success)
                {
                    _activityLog.AddEntry("Task completed: '" + task.Title + "'");
                    return "✅ Task '" + task.Title + "' marked as completed! Great work, " + UserName + "!";
                }
                else
                {
                    return "Sorry, I couldn't update that task. Please try again.";
                }
            }
            catch (Exception)
            {
                return "Please specify a valid task number. For example: 'complete task 1'";
            }
        }

        private string HandleDeleteTask(string lower)
        {
            try
            {
                string[] parts = lower.Split(' ');
                int taskNumber = int.Parse(parts[parts.Length - 1]);

                List<TaskItem> tasks = _database.GetAllTasks();

                if (taskNumber < 1 || taskNumber > tasks.Count)
                    return "Task number " + taskNumber + " doesn't exist. Type 'view tasks' to see your tasks.";

                TaskItem task = tasks[taskNumber - 1];
                bool success = _database.DeleteTask(task.Id);

                if (success)
                {
                    _activityLog.AddEntry("Task deleted: '" + task.Title + "'");
                    return "🗑️ Task '" + task.Title + "' has been deleted.";
                }
                else
                {
                    return "Sorry, I couldn't delete that task. Please try again.";
                }
            }
            catch (Exception)
            {
                return "Please specify a valid task number. For example: 'delete task 1'";
            }
        }

        // ── Sentiment detection (from Part 2) ────────────────────────────────
        private string? DetectSentiment(string lower)
        {
            foreach (string keyword in _worriedKeywords)
            {
                if (lower.Contains(keyword))
                {
                    _activityLog.AddEntry("Sentiment detected: worried");
                    return "It's completely understandable to feel that way, " + UserName + ". " +
                           "Cybersecurity can feel overwhelming but you are already taking the right step by learning about it. " +
                           "Would you like to know more about password safety or any other topic?";
                }
            }

            foreach (string keyword in _frustratedKeywords)
            {
                if (lower.Contains(keyword))
                {
                    _activityLog.AddEntry("Sentiment detected: frustrated");
                    return "I understand this can feel confusing, " + UserName + ". Let me try to explain things more clearly. " +
                           "Try typing 'password' or 'phishing' and I will give you a clear explanation.";
                }
            }

            return null;
        }

        // ── Follow-up handling (from Part 2) ─────────────────────────────────
        private bool IsFollowUp(string lower)
        {
            foreach (string keyword in _followUpKeywords)
            {
                if (lower.Contains(keyword))
                    return true;
            }
            return false;
        }

        private string GetFollowUpResponse()
        {
            switch (_lastTopic)
            {
                case "phishing": return "Here is another phishing tip:\n" + GetRandomResponse(_phishingResponses);
                case "password": return "Here is another password tip:\n" + GetRandomResponse(_passwordResponses);
                case "privacy": return "Here is another privacy tip:\n" + GetRandomResponse(_privacyResponses);
                case "browsing": return "Here is another browsing tip:\n" + GetRandomResponse(_browsingResponses);
                case "malware":
                    return "More on malware, " + UserName + ":\n" +
                           "Ransomware encrypts your files and demands payment. " +
                           "Always use the 3-2-1 backup rule: 3 copies, 2 storage types, 1 offsite.";
                case "2fa":
                    return "More on 2FA, " + UserName + ":\n" +
                           "Hardware keys like YubiKey are the most secure. " +
                           "Authenticator apps are second. SMS codes are least secure but still better than nothing.";
                default:
                    return "Could you remind me which topic you'd like more on? Try typing the topic name.";
            }
        }

        // ── Memory helpers ───────────────────────────────────────────────────
        private void StoreFavouriteTopic(string topic)
        {
            if (FavouriteTopic == null)
                FavouriteTopic = topic;
        }

        private string GetMemoryRecall()
        {
            string recall = "Here is what I remember about you, " + UserName + ":\n";
            recall += "• Your name: " + UserName + "\n";
            if (FavouriteTopic != null)
                recall += "• Your favourite topic: " + FavouriteTopic + "\n";
            if (_lastTopic != null)
                recall += "• Last topic discussed: " + _lastTopic + "\n";
            recall += "• Activity log entries: " + _activityLog.GetCount();
            return recall;
        }

        // ── Random response picker ───────────────────────────────────────────
        private string GetRandomResponse(List<string> responses)
        {
            int index = _random.Next(0, responses.Count);
            return responses[index] ?? string.Empty;
        }

        // ── Help message ─────────────────────────────────────────────────────
        private string GetHelpMessage()
        {
            return "Here is everything I can help you with, " + UserName + ":\n\n" +
                   "💬 CYBERSECURITY TOPICS\n" +
                   "  • password, phishing, browsing, malware, privacy, 2fa, vpn, breach\n\n" +
                   "📋 TASK ASSISTANT\n" +
                   "  • add task - [task name]     Add a cybersecurity task\n" +
                   "  • remind me to [action]      Add a task with a reminder\n" +
                   "  • view tasks                 See all your tasks\n" +
                   "  • complete task [number]     Mark a task as done\n" +
                   "  • delete task [number]       Remove a task\n\n" +
                   "🎮 QUIZ\n" +
                   "  • start quiz                 Take the cybersecurity quiz\n\n" +
                   "📜 ACTIVITY LOG\n" +
                   "  • show activity log          See recent actions\n\n" +
                   "🧠 MEMORY\n" +
                   "  • what do you remember       See what I know about you";
        }
    }
}