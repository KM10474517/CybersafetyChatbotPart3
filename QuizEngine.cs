using System.Collections.Generic;

namespace CybersafetyChatbotPart3
{
    // Represents a single quiz question
    public class QuizQuestion
    {
        public string Question { get; set; } = "";
        public List<string> Options { get; set; } = new List<string>();
        public string CorrectAnswer { get; set; } = "";
        public string Explanation { get; set; } = "";
    }

    // Handles all quiz logic including questions, scoring, and feedback
    public class QuizEngine
    {
        // Current question index
        private int _currentIndex = 0;

        // Score tracker
        public int Score { get; private set; } = 0;
        public int TotalQuestions { get; private set; } = 0;

        // Whether the quiz is currently active
        public bool IsActive { get; private set; } = false;

        // List of all quiz questions
        private List<QuizQuestion> _questions = new List<QuizQuestion>
        {
            new QuizQuestion
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report it as phishing", "D) Ignore it" },
                CorrectAnswer = "C",
                Explanation = "Correct! Reporting phishing emails helps prevent scams and protects others."
            },
            new QuizQuestion
            {
                Question = "How long should a strong password be?",
                Options = new List<string> { "A) At least 4 characters", "B) At least 6 characters", "C) At least 8 characters", "D) At least 12 characters" },
                CorrectAnswer = "D",
                Explanation = "Correct! A strong password should be at least 12 characters long with letters, numbers and symbols."
            },
            new QuizQuestion
            {
                Question = "What does HTTPS stand for?",
                Options = new List<string> { "A) HyperText Transfer Protocol Secure", "B) High Transfer Text Protocol Safe", "C) Hyper Transfer Text Process Secure", "D) None of the above" },
                CorrectAnswer = "A",
                Explanation = "Correct! HTTPS means the connection between your browser and the website is encrypted."
            },
            new QuizQuestion
            {
                Question = "True or False: You should use the same password for all your accounts.",
                Options = new List<string> { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Correct! Never reuse passwords. If one account is hacked, all others become vulnerable."
            },
            new QuizQuestion
            {
                Question = "What is two-factor authentication (2FA)?",
                Options = new List<string> { "A) Using two passwords", "B) A second verification step after your password", "C) Logging in from two devices", "D) Using two email addresses" },
                CorrectAnswer = "B",
                Explanation = "Correct! 2FA adds a second step — like a code from your phone — making it much harder for hackers to access your account."
            },
            new QuizQuestion
            {
                Question = "What is malware?",
                Options = new List<string> { "A) A type of antivirus software", "B) A secure website", "C) Malicious software designed to harm your device", "D) A strong password" },
                CorrectAnswer = "C",
                Explanation = "Correct! Malware includes viruses, ransomware, spyware and trojans designed to damage or steal data."
            },
            new QuizQuestion
            {
                Question = "True or False: Public Wi-Fi is always safe to use for banking.",
                Options = new List<string> { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Correct! Public Wi-Fi is not secure. Use a VPN or avoid sensitive activities on public networks."
            },
            new QuizQuestion
            {
                Question = "What does a VPN do?",
                Options = new List<string> { "A) Makes your internet faster", "B) Encrypts your internet traffic and hides your IP address", "C) Removes viruses from your device", "D) Blocks all websites" },
                CorrectAnswer = "B",
                Explanation = "Correct! A VPN encrypts your traffic and protects your identity online, especially on public Wi-Fi."
            },
            new QuizQuestion
            {
                Question = "What is phishing?",
                Options = new List<string> { "A) A type of antivirus", "B) A secure login method", "C) A trick to steal personal information via fake emails or websites", "D) A way to speed up your computer" },
                CorrectAnswer = "C",
                Explanation = "Correct! Phishing attacks use fake emails or websites to trick you into giving away personal information."
            },
            new QuizQuestion
            {
                Question = "True or False: You should click links in emails even if you don't know the sender.",
                Options = new List<string> { "A) True", "B) False" },
                CorrectAnswer = "B",
                Explanation = "Correct! Never click links from unknown senders. Always verify the sender before clicking any link."
            },
            new QuizQuestion
            {
                Question = "What should you do if your data has been breached?",
                Options = new List<string> { "A) Do nothing", "B) Change your passwords and enable 2FA immediately", "C) Delete your accounts", "D) Restart your computer" },
                CorrectAnswer = "B",
                Explanation = "Correct! Change your passwords immediately and enable 2FA to protect your accounts after a breach."
            }
        };

        // ── Start the quiz ───────────────────────────────────────────────────
        public void StartQuiz()
        {
            _currentIndex = 0;
            Score = 0;
            TotalQuestions = _questions.Count;
            IsActive = true;
        }

        // ── Get the current question ─────────────────────────────────────────
        public QuizQuestion? GetCurrentQuestion()
        {
            if (_currentIndex < _questions.Count)
                return _questions[_currentIndex];
            return null;
        }

        // ── Check if the quiz is finished ────────────────────────────────────
        public bool IsFinished()
        {
            return _currentIndex >= _questions.Count;
        }

        // ── Submit an answer and get feedback ────────────────────────────────
        public string SubmitAnswer(string answer)
        {
            if (_currentIndex >= _questions.Count)
                return "The quiz is already finished.";

            QuizQuestion current = _questions[_currentIndex];
            string userAnswer = answer.Trim().ToUpper();
            string feedback = "";

            if (userAnswer == current.CorrectAnswer.ToUpper())
            {
                Score++;
                feedback = "✅ " + current.Explanation;
            }
            else
            {
                feedback = "❌ Wrong! The correct answer was " + current.CorrectAnswer + ". " + current.Explanation;
            }

            _currentIndex++;
            return feedback;
        }

        // ── Get the final score message ──────────────────────────────────────
        public string GetFinalScore()
        {
            IsActive = false;
            double percentage = (double)Score / TotalQuestions * 100;
            string result = "🏆 Quiz Complete! You scored " + Score + " out of " + TotalQuestions + ".\n\n";

            if (percentage >= 80)
                result += "Great job! You're a cybersecurity pro! 🎉";
            else if (percentage >= 50)
                result += "Good effort! Keep learning to stay safe online. 📚";
            else
                result += "Keep learning! Cybersecurity knowledge is your best defence. 💪";

            return result;
        }
    }
}