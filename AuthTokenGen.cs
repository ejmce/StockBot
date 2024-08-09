using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Reddit;
using Reddit.Controllers;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.RegularExpressions;
using Azure;
using Azure.AI.TextAnalytics;
using Newtonsoft.Json;
using Reddit.Models;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Client;
using Azure.Core;
using System.IO;
using System.Net;
using Tweetinvi.Core.Models;
using Reddit.Exceptions;
using Newtonsoft.Json.Linq;
using Reddit.AuthTokenRetriever;
using System.Diagnostics;
using System.Net.Sockets;
using uhttpsharp.Listeners;
using uhttpsharp.RequestProviders;
using uhttpsharp;
using Reddit.AuthTokenRetriever.EventArgs;
using System.Reflection;
using RestSharp;
using Reddit.Things;

namespace StockBot
{
    internal class AuthTokenGen
    {
        public async Task<string> GetTokensAsync(string RedirectUri, string AppId, string AppSecret)
        {
            // Create a new instance of the auth token retrieval library.  --Kris
            AuthTokenRetrieverLib authTokenRetrieverLib = new AuthTokenRetrieverLib(AppId, 8080, "localhost", RedirectUri, AppSecret);
            GetAuthToken(RedirectUri, AppId, authTokenRetrieverLib);
            string authorizationCode = await StartHttpListener(RedirectUri);
            // Use the captured authorization code to request tokens
            var tokens = await GetTokensAsync(AppId, AppSecret, authorizationCode, RedirectUri);
            // Use AuthTokenRetrieverLib to get the access and refresh tokens
            //var tokens = await Reddit.AuthTokenRetriever.GetTokensAsync(AppId, AppSecret, authorizationCode, RedirectUri);
            //authTokenRetrieverLib.AwaitCallback();
            //authTokenRetrieverLib.StopListening();
            // Use Reddit.AuthTokenRetrieverLib to get the access and refresh tokens
            //var tokenResponse = await authTokenRetrieverLib.GetTokensAsync(ClientId, ClientSecret, authorizationCode, RedirectUri);

            //var refreshToken = authTokenRetrieverLib.RefreshToken;
            // string authToken = authorizationCode;
            // Save the tokens for future use
            string refreshToken = tokens["refresh_token"];
            return refreshToken;
        }

        public void GetAuthToken(string RedirectUri, string AppId, AuthTokenRetrieverLib authTokenRetrieverLib)
        {
            // Start the callback listener.  --Kris
            // Note - Ignore the logging exception message if you see it.  You can use Console.Clear() after this call to get rid of it if you're running a console app.
            //authTokenRetrieverLib.AwaitCallback();
            //AwaitCallback(authTokenRetrieverLib);

            // Open the browser to the Reddit authentication page.  Once the user clicks "accept", Reddit will redirect the browser to localhost:8080, where AwaitCallback will take over.  --Kris
            string state = Guid.NewGuid().ToString();
            string scope = "creddits%20modcontributors%20modmail%20modconfig%20subscribe%20structuredstyles%20vote%20wikiedit%20mysubreddits%20submit%20modlog%20modposts%20modflair%20save%20modothers%20read%20privatemessages%20report%20identity%20livemanage%20account%20modtraffic%20wikiread%20edit%20modwiki%20modself%20history%20flair";
            string authorizationUrl = $"https://www.reddit.com/api/v1/authorize?client_id={AppId}&response_type=code&state={state}&redirect_uri={RedirectUri}&duration=permanent&scope={scope}";
            //Console.WriteLine("Open the following URL in your browser to authorize the app:");
            OpenBrowser(authorizationUrl);
            Console.WriteLine("Waiting...");          

            //string state = Guid.NewGuid().ToString(); // Generate a random state string
            //string authorizationUrl = $"https://www.reddit.com/api/v1/authorize?client_id={AppId}&response_type=code&state={state}&redirect_uri={RedirectUri}&duration=permanent&scope=read";

            //Console.WriteLine("Open the following URL in your browser to authorize the app:");
            //Console.WriteLine(authorizationUrl);


            //// Store the state to validate later (e.g., in a database, session, etc.)
            //// Here, for simplicity, we are just printing it out
            //Console.WriteLine("State: " + state);
        }

        public static void OpenBrowser(string authUrl, string browserPath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe")
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(authUrl);
                Process.Start(processStartInfo);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // This typically occurs if the runtime doesn't know where your browser is.  Use BrowserPath for when this happens.  --Kris
                ProcessStartInfo processStartInfo = new ProcessStartInfo(browserPath)
                {
                    Arguments = authUrl
                };
                Process.Start(processStartInfo);
            }
        }
        public async Task<string> StartHttpListener(string RedirectUri)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(RedirectUri);
            listener.Start();

            Console.WriteLine("Listening for requests at " + RedirectUri);

            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            string responseString = "<html><body>Authorization successful. You can close this window.</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();

            listener.Stop();

            return request.QueryString["code"];
        }

        internal int Port { get; private set; }

        internal string Host { get; private set; }

        internal HttpServer HttpServer { get; private set; }

        public string AccessToken { get; private set; }

        public string RefreshToken { get; private set; }

        public event EventHandler<AuthSuccessEventArgs> AuthSuccess;

        //public async void AwaitCallback(AuthTokenRetrieverLib authTokenRetrieverLib, bool generateLocalOutput = false, string Host = "localHost", int Port = 8080)
        //{
        //    await Task.Run(() =>
        //    {
        //        HttpServer httpServer2 = (HttpServer = new HttpServer(new HttpRequestProvider()));
        //        using (httpServer2)
        //        {
        //            HttpServer.Use(new TcpListenerAdapter(new TcpListener(IPAddress.Parse(Host.Equals("localost") ? IPAddress.Loopback.ToString() : Host), Port)));
        //            HttpServer.Use(delegate (IHttpContext context, Func<Task> next)
        //            {
        //                string text = null;
        //                string text2 = null;
        //                try
        //                {
        //                    text = context.Request.QueryString.GetByName("code");
        //                    text2 = context.Request.QueryString.GetByName("state");
        //                }
        //                catch (KeyNotFoundException)
        //                {
        //                    context.Response = new uhttpsharp.HttpResponse(HttpResponseCode.Ok, Encoding.UTF8.GetBytes("<b>ERROR:  No code and/or state received!</b>"), keepAliveConnection: false);
        //                    throw new Exception("ERROR:  Request received without code and/or state!");
        //                }

        //                if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
        //                {
        //                    RestRequest restRequest = new RestRequest("/api/v1/access_token", Method.POST);
        //                    restRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(text2)));
        //                    restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        //                    restRequest.AddParameter("grant_type", "authorization_code");
        //                    restRequest.AddParameter("code", text);
        //                    restRequest.AddParameter("redirect_uri", "http://" + Host + ":" + Port + "/Reddit.NET/oauthRedirect");
        //                    OAuthToken oAuthToken = JsonConvert.DeserializeObject<OAuthToken>(authTokenRetrieverLib.ExecuteRequest(restRequest));
        //                    AccessToken = oAuthToken.AccessToken;
        //                    RefreshToken = oAuthToken.RefreshToken;
        //                    this.AuthSuccess?.Invoke(this, new AuthSuccessEventArgs
        //                    {
        //                        AccessToken = oAuthToken.AccessToken,
        //                        RefreshToken = oAuthToken.RefreshToken
        //                    });
        //                    string[] array = text2.Split(':');
        //                    if (array == null || array.Length == 0)
        //                    {
        //                        throw new Exception("State must consist of 'appId:appSecret'!");
        //                    }

        //                    string text3 = array[0];
        //                    string text4 = ((array.Length >= 2) ? array[1] : null);
        //                    string text5;
        //                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AuthTokenRetrieverLib.Templates.Success.html"))
        //                    {
        //                        StreamReader streamReader = new StreamReader(stream);
        //                        text5 = streamReader.ReadToEnd();
        //                    }

        //                    text5 = text5.Replace("REDDIT_OAUTH_ACCESS_TOKEN", oAuthToken.AccessToken);
        //                    text5 = text5.Replace("REDDIT_OAUTH_REFRESH_TOKEN", oAuthToken.RefreshToken);
        //                    if (generateLocalOutput)
        //                    {
        //                        string text6;
        //                        using (Stream stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("AuthTokenRetrieverLib.Templates.TokenSaved.html"))
        //                        {
        //                            StreamReader streamReader2 = new StreamReader(stream2);
        //                            text6 = streamReader2.ReadToEnd();
        //                        }

        //                        string text7 = "." + text3 + "." + ((!string.IsNullOrWhiteSpace(text4)) ? (text4 + ".") : "") + "json";
        //                        string text8 = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "RDNOauthToken_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + text7;
        //                        File.WriteAllText(text8, JsonConvert.SerializeObject(oAuthToken));
        //                        text5 = text5.Replace("TOKEN_SAVED", text6.Replace("LOCAL_TOKEN_PATH", text8));
        //                    }
        //                    else
        //                    {
        //                        text5 = text5.Replace("TOKEN_SAVED", "");
        //                    }

        //                    context.Response = new uhttpsharp.HttpResponse(HttpResponseCode.Ok, Encoding.UTF8.GetBytes(text5), keepAliveConnection: false);
        //                }

        //                return Task.Factory.GetCompleted();
        //            });
        //            HttpServer.Start();
        //        }
        //    });
        //}

        private static async Task<Dictionary<string, string>> GetTokensAsync(string clientId, string clientSecret, string authorizationCode, string redirectUri)
        {
            using (var client = new HttpClient())
            {
                var authToken = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (https://myappwebsite.com)");

                var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", authorizationCode },
                { "redirect_uri", redirectUri }
            };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync("https://www.reddit.com/api/v1/access_token", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var tokens = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
                    string refreshToken = tokens["refresh_token"];
                    
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyApp/1.0 (https://myappwebsite.com)");

                    var response1 = await client.GetAsync($"https://oauth.reddit.com/r/pennystocks/top?limit=100");
                    var responseString1 = await response.Content.ReadAsStringAsync();

                    if (response1.IsSuccessStatusCode)
                    {
                        var jsonResponse = JObject.Parse(responseString1);
                        var posts = new List<string>();

                        foreach (var post in jsonResponse["data"]["children"])
                        {
                            posts.Add(post["data"]["title"].ToString());
                        }

                        var test = posts;
                    }
                    else
                    {
                        Console.WriteLine("Error: " + responseString);
                        throw new Exception("Failed to retrieve posts.");
                    }

                    return tokens;
                }
                else
                {
                    Console.WriteLine("Error: " + responseString);
                    throw new Exception("Failed to retrieve tokens.");
                }
            }
        }
    }
}
