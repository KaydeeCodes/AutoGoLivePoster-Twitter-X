using System;
using System.Net;
using System.Threading.Tasks;

namespace GoLivePoster
{
    public static class LocalHttpServer
    {
        public static async Task<string> WaitForCodeAsync(string expectedState)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/callback/");
            listener.Start();

            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            var code = request.QueryString["code"];
            var state = request.QueryString["state"];

            string responseString;

            if (state != expectedState)
            {
                responseString = "State mismatch. Authentication failed.";
                code = null;
            }
            else
            {
                responseString = "Authentication successful. You can close this window.";
            }

            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();
            listener.Stop();

            return code;
        }
    }
}