using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoLivePoster
{
    public class TwitterClient
    {
        private readonly string _accessToken;

        public TwitterClient(string accessToken)
        {
            _accessToken = accessToken;
        }

        public async Task PostTweetAsync(string text)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var payload = new { text };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.twitter.com/2/tweets", content);
            var responseText = await response.Content.ReadAsStringAsync();

            Console.WriteLine("📥 Tweet API response:");
            Console.WriteLine(responseText);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error posting tweet: {response.StatusCode}, {responseText}");
            }
        }

        public static async Task<TokenInfo?> RefreshAccessTokenAsync(string refreshToken, string clientId)
        {
            using var client = new HttpClient();

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("client_id", clientId)
            });

            Console.WriteLine("🔁 Refreshing access token...");
            var response = await client.PostAsync("https://api.twitter.com/2/oauth2/token", requestContent);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("📥 Refresh response:");
            Console.WriteLine(body);

            if (!response.IsSuccessStatusCode)
                return null;

            var doc = JsonDocument.Parse(body);
            return new TokenInfo
            {
                AccessToken = doc.RootElement.GetProperty("access_token").GetString(),
                RefreshToken = doc.RootElement.TryGetProperty("refresh_token", out var r)
                    ? r.GetString()
                    : refreshToken
            };
        }

        public static async Task<TokenInfo?> ExchangeCodeForTokenAsync(string authCode, string codeVerifier, string clientId, string redirectUri)
        {
            using var client = new HttpClient();

            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("code_verifier", codeVerifier),
                new KeyValuePair<string, string>("code", authCode)
            });

            Console.WriteLine("🔁 Exchanging auth code for access token...");
            var response = await client.PostAsync("https://api.twitter.com/2/oauth2/token", requestContent);
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine("📥 Token response:");
            Console.WriteLine(body);

            if (!response.IsSuccessStatusCode)
                return null;

            var doc = JsonDocument.Parse(body);
            return new TokenInfo
            {
                AccessToken = doc.RootElement.GetProperty("access_token").GetString(),
                RefreshToken = doc.RootElement.GetProperty("refresh_token").GetString()
            };
        }
    }
}
