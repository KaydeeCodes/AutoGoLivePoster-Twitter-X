using System;
using System.IO;
using System.Text.Json;

namespace GoLivePoster
{
    public class Settings
    {
        public string? ClientId { get; set; }
        public string? RedirectUri { get; set; }

        public static Settings Load(string path = "settings.json")
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("⚠️  settings.json not found.");
                CreateTemplate(path);
                Console.WriteLine("📄 A template settings.json file has been created.");
                Console.WriteLine("✏️  Please edit it with your Twitter App info, then run again.");
                Environment.Exit(1);
            }

            var json = File.ReadAllText(path);
            var settings = JsonSerializer.Deserialize<Settings>(json);

            if (string.IsNullOrWhiteSpace(settings?.ClientId) || string.IsNullOrWhiteSpace(settings.RedirectUri))
            {
                Console.WriteLine("⚠️  settings.json is incomplete.");
                Console.WriteLine("✏️  Please fill in your 'clientId' and 'redirectUri' values.");
                Environment.Exit(1);
            }

            return settings;
        }

        private static void CreateTemplate(string path)
        {
            var template = new Settings
            {
                ClientId = "your-twitter-client-id",
                RedirectUri = "http://localhost:5000/callback/"
            };

            var json = JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}