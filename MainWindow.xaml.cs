using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersafetyChatbotPart3
{
    public partial class MainWindow : Window
    {
        // The chatbot engine
        private ChatEngine _chatEngine;

        // Tracks whether the user has entered their name yet
        private bool _nameEntered = false;

        public MainWindow()
        {
            InitializeComponent();

            // Create the chat engine
            _chatEngine = new ChatEngine();

            // Play the voice greeting
            VoiceGreeting.Play();

            // Show the welcome messages
            ShowBotMessage("Hello! Welcome to the Cybersecurity Awareness Bot. 🔐");
            ShowBotMessage("I'm here to help you stay safe online.");
            ShowBotMessage("I can help you with cybersecurity tips, manage your security tasks, quiz you on cybersecurity, and keep an activity log.");
            ShowBotMessage("Before we begin, what is your name?");

            TxtInput.Focus();
        }

        // ── Button events ────────────────────────────────────────────────────

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessInput();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ChatPanel.Children.Clear();
            ShowBotMessage("Chat cleared. How can I help you?");
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ── Quick action button events ───────────────────────────────────────

        private void BtnViewTasks_Click(object sender, RoutedEventArgs e)
        {
            if (!_nameEntered) return;
            ShowUserMessage("view tasks");
            string response = _chatEngine.GetResponse("view tasks");
            ShowBotMessage(response);
            UpdateMemoryBar();
        }

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            if (!_nameEntered) return;
            TxtInput.Text = "add task - ";
            TxtInput.Focus();
            TxtInput.CaretIndex = TxtInput.Text.Length;
        }

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (!_nameEntered) return;
            ShowUserMessage("start quiz");
            string response = _chatEngine.GetResponse("start quiz");
            ShowBotMessage(response);
            UpdateMemoryBar();
        }

        private void BtnActivityLog_Click(object sender, RoutedEventArgs e)
        {
            if (!_nameEntered) return;
            ShowUserMessage("show activity log");
            string response = _chatEngine.GetResponse("show activity log");
            ShowBotMessage(response);
            UpdateMemoryBar();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            if (!_nameEntered) return;
            ShowUserMessage("help");
            string response = _chatEngine.GetResponse("help");
            ShowBotMessage(response);
        }

        // ── Main input processor ─────────────────────────────────────────────

        private void ProcessInput()
        {
            string input = TxtInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                TxtStatus.Text = "Please type a message first.";
                return;
            }

            ShowUserMessage(input);
            TxtInput.Clear();
            TxtInput.Focus();

            // Save the user's name on the first message
            if (!_nameEntered)
            {
                _chatEngine.SetUserName(input);
                _nameEntered = true;
                ShowBotMessage("Nice to meet you, " + _chatEngine.UserName + "! 😊");
                ShowBotMessage("Type 'help' to see everything I can do, or just ask me anything!");
                UpdateMemoryBar();
                return;
            }

            string response = _chatEngine.GetResponse(input);
            ShowBotMessage(response);
            UpdateMemoryBar();
            TxtStatus.Text = "Ready";
        }

        // ── Display a bot message bubble ─────────────────────────────────────

        private void ShowBotMessage(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 27, 34)),
                CornerRadius = new CornerRadius(12, 12, 12, 2),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 4, 80, 4),
                MaxWidth = 650
            };

            StackPanel stack = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "🤖 Bot",
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136)),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };

            stack.Children.Add(label);
            stack.Children.Add(text);
            bubble.Child = stack;

            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();
        }

        // ── Display a user message bubble ────────────────────────────────────

        private void ShowUserMessage(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 100, 55)),
                CornerRadius = new CornerRadius(12, 12, 2, 12),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(80, 4, 0, 4),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 550
            };

            StackPanel stack = new StackPanel();

            TextBlock label = new TextBlock
            {
                Text = "👤 " + (_chatEngine.UserName ?? "You"),
                Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136)),
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 4)
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 237, 243)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            };

            stack.Children.Add(label);
            stack.Children.Add(text);
            bubble.Child = stack;

            ChatPanel.Children.Add(bubble);
            ChatScrollViewer.ScrollToBottom();
        }

        // ── Update the memory status bar ─────────────────────────────────────

        private void UpdateMemoryBar()
        {
            if (_chatEngine.FavouriteTopic != null)
                TxtMemory.Text = "💾 " + _chatEngine.UserName + " | Topic: " + _chatEngine.FavouriteTopic;
            else if (_chatEngine.UserName != null)
                TxtMemory.Text = "💾 " + _chatEngine.UserName;
        }
    }
}