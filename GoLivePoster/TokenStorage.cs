using System;
using System.IO;
using System.Text.Json;

namespace GoLivePoster
{
    public class TokenInfo
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public static class TokenStorage
    {
        private static string FilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".goliveposter", "tokens.json");

        public static void Save(TokenInfo token)
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            var json = JsonSerializer.Serialize(token, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static TokenInfo? Load()
        {
            if (!File.Exists(FilePath))
                return null;

            try
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<TokenInfo>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}