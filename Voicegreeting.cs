using System;
using System.IO;
using System.Media;

namespace CybersafetyChatbotPart3
{
    // Plays the WAV voice greeting when the application starts
    public static class VoiceGreeting
    {
        public static void Play()
        {
            try
            {
                string wavPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ChatbotGreeting.wav");

                if (File.Exists(wavPath))
                {
                    SoundPlayer player = new SoundPlayer(wavPath);
                    player.PlaySync();
                }
            }
            catch (Exception)
            {
                // Audio unavailable - continue silently
            }
        }
    }
}