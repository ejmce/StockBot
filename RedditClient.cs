using Azure.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Parameters;
using uhttpsharp.Clients;
using StockBot;

namespace StockBot
{
    internal class RedditClient
    {
        HttpClient reddit = new HttpClient();
        string RefreshToken = null;
        public RedditClient(string refreshToken)
        {
            RefreshToken = refreshToken;
        }

        public async Task<List<string>> GetPosts(string subreddit, int count)
        {
            reddit.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", RefreshToken);
            reddit.DefaultRequestHeaders.Add("User-Agent", "MockClient/0.1 by Ej");
            var response = await reddit.GetAsync($"https://oauth.reddit.com/r/{subreddit}/top?limit={count}");
            var responseString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = JObject.Parse(responseString);
                var posts = new List<string>();

                foreach (var post in jsonResponse["data"]["children"])
                {
                    posts.Add(post["data"]["title"].ToString());
                }

                return posts;
            }
            else
            {
                Console.WriteLine("Error: " + responseString);
                throw new Exception("Failed to retrieve posts.");
            }
        }
    }
}
