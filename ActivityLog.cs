using System;
using System.Collections.Generic;

namespace CybersafetyChatbotPart3
{
    // Records all significant actions the chatbot takes during a session
    public class ActivityLog
    {
        // List that stores all log entries
        private List<string> _log = new List<string>();

        // ── Add an entry to the log ──────────────────────────────────────────
        public void AddEntry(string action)
        {
            // Add a timestamp alongside the action
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _log.Add("[" + timestamp + "] " + action);
        }

        // ── Get the last 10 log entries ──────────────────────────────────────
        public string GetLog()
        {
            if (_log.Count == 0)
                return "No actions recorded yet.";

            // Show only the last 10 entries to keep it concise
            int startIndex = Math.Max(0, _log.Count - 10);
            string result = "Here is a summary of recent actions:\n\n";

            for (int i = startIndex; i < _log.Count; i++)
            {
                result += (i - startIndex + 1) + ". " + _log[i] + "\n";
            }

            return result;
        }

        // ── Get the total number of logged actions ───────────────────────────
        public int GetCount()
        {
            return _log.Count;
        }
    }
}