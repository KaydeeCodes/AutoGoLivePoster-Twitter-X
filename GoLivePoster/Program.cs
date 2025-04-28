﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GoLivePoster;

class Program
{
    static async Task Main(string[] args)
    {
        string platform = "twitch";
        string streamLink = "https://twitch.tv/kaydeecodes";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--platform" && i + 1 < args.Length)
                platform = args[i + 1];
            if (args[i] == "--link" && i + 1 < args.Length)
                streamLink = args[i + 1];
        }

        string tweetText = $"🔴 I'm LIVE now on {platform.ToUpper()}! Come hang out 👉 {streamLink} 🎮 {DateTime.Now:T} <3 ";

        var settings = Settings.Load();
        var clientId = settings.ClientId!;
        var redirectUri = settings.RedirectUri!;
        
        // Try to load saved tokens
        var saved = TokenStorage.Load();
        TokenInfo? tokenInfo = null;

        if (saved?.RefreshToken != null)
        {
            tokenInfo = await TwitterClient.RefreshAccessTokenAsync(saved.RefreshToken, clientId);
            if (tokenInfo != null)
            {
                Console.WriteLine("✅ Token refreshed from saved refresh token!");
                TokenStorage.Save(tokenInfo);
            }
        }

        if (tokenInfo == null)
        {
            Console.WriteLine("🌐 No valid saved token. Starting browser-based login...");

            var codeVerifier = Pkce.GenerateCodeVerifier();
            var codeChallenge = Pkce.GenerateCodeChallenge(codeVerifier);
            var scope = "tweet.read tweet.write users.read offline.access";
            var state = Guid.NewGuid().ToString();

            var authUrl = $"https://twitter.com/i/oauth2/authorize" +
                          $"?response_type=code" +
                          $"&client_id={clientId}" +
                          $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                          $"&scope={Uri.EscapeDataString(scope)}" +
                          $"&state={state}" +
                          $"&code_challenge={codeChallenge}" +
                          $"&code_challenge_method=S256";

            OpenBrowser(authUrl);

            var authCode = await LocalHttpServer.WaitForCodeAsync(state);
            if (string.IsNullOrEmpty(authCode))
            {
                Console.WriteLine("❌ No auth code received.");
                return;
            }

            tokenInfo = await TwitterClient.ExchangeCodeForTokenAsync(authCode, codeVerifier, clientId, redirectUri);
            if (tokenInfo == null)
            {
                Console.WriteLine("❌ Token exchange failed.");
                return;
            }

            TokenStorage.Save(tokenInfo);
        }

        var twitter = new TwitterClient(tokenInfo!.AccessToken!);
        try
        {
            Console.WriteLine($"📢 Sending tweet:\n{tweetText}");
            await twitter.PostTweetAsync(tweetText);
            Console.WriteLine("✅ Tweet posted!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

    static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            throw new PlatformNotSupportedException("Can't open browser.");
        }
    }
}
