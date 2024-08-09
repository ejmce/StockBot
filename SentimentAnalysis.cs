using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reddit;
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
using System.Net.Http;
using Tweetinvi.Core.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Tweetinvi.Core.Models.Properties;
using Reddit.AuthTokenRetriever;
using Reddit.Controllers;


namespace StockBot
{
    public class RedditSentimentAnalysis
    {
        private const string AppId = " ";
        private const string AppSecret = " ";
        private string RefreshToken = " ";
        private const string RedirectUri = " ";
        //private const string AuthCode = " ";
        private int connCount = 0;
        //private static readonly HttpClient client = new HttpClient();
        //                    RefreshToken = await GetRefreshToken(); 

        private RedditClient reddit;

        public RedditSentimentAnalysis()
        {
            reddit = new RedditClient(RefreshToken);
        }
        
        public async Task<string> GetRefreshToken()
        {
            // GetAuthToken if needed
            AuthTokenGen getTokensAsync = new AuthTokenGen();
            string token = await getTokensAsync.GetTokensAsync(RedirectUri, AppId, AppSecret);
            Console.WriteLine("Replace the refresh token with: " + token);

            return token;
        }

        public async Task<List<string>> AnalyzeRedditAsync()
        {
            RefreshToken = await GetRefreshToken();
            var subreddits = new List<string>();
            int count = 100;
            try
            {

                string pennyStocks = "pennystocks";
                //var stocks = reddit.Subreddit("stocks").Posts.GetHot(limit: 50);
                //var wallStreetBets = reddit.Subreddit("wallstreetbets").Posts.GetHot(limit: 100);
                //var investing = reddit.Subreddit("investing").Posts.GetHot(limit: 50);
                //var options = reddit.Subreddit("options").Posts.GetHot(limit: 50);
                string dayTrading = "DayTrading";
                //var swingTrading = reddit.Subreddit("swingtrading").Posts.GetHot(limit: 50);

                subreddits.Add(pennyStocks);
                //subreddit.AddRange(stocks);
                //subreddit.AddRange(wallStreetBets);
                //subreddit.AddRange(investing);
                //subreddit.AddRange(options);
                subreddits.Add(dayTrading);
                //subreddit.AddRange(swingTrading);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                // Handle other exceptions
                throw;
            }

            var sentiments = new List<string>();
            var posts = new List<string>();
            try
            {
                foreach (var subreddit in subreddits)
                {
                    var returnedPosts = await reddit.GetPosts(subreddit, count);
                    posts.Append(returnedPosts.ToString());
                }
            }
            catch
            {
                throw;
            }

            foreach (var post in posts)
            {
                var tickers = await ExtractRedditStockTickersAsync(post);
                foreach (var ticker in tickers)
                {
                    //var sentiment = await AnalyzeSentimentAsync(post, ticker);
                    //if (sentiment[1] == "Neutral")
                    //{
                    //    sentiments.Add($"{ticker}: {sentiment[1]}");                        
                    //}
                }
            }

            Console.WriteLine(sentiments);
            return sentiments;
        }

        private async Task<List<string>> ExtractRedditStockTickersAsync(string text)
        {
            return await Task.Run(() =>
            {
                // Regex to find stock tickers (1-5 all-caps letters)
                var tickerPattern = new Regex(@"\b(?!IPO|AI|CPI|LLC|SEC|CEO|TO|PM|MUSK|ATH|PPI|ER|AM)\$?[A-Z]{2,5}\b");
                var matches = tickerPattern.Matches(text);

                // Convert MatchCollection to List<string>
                var tickers = new List<string>();
                foreach (Match match in matches)
                {
                    tickers.Add(match.Value);
                }

                return tickers.Distinct().ToList();
            });
        }

    //    private async Task<List<string>> AnalyzeSentimentAsync(string post, string ticker)
    //    {
    //        // Simulate asynchronous sentiment analysis
    //        await Task.Delay(500);
    //        var text = post;

    //        // For simplicity, use a basic keyword-based sentiment analysis.
    //        var positiveWords = new List<string> { "good", "great", "positive", "up", "bullish", "moon", "calls", "stonks", 
    //            "no debt", "buy", "beat", "undervalued", "green", "gain", "purchase" };
    //        var negativeWords = new List<string> { "bad", "terrible", "negative", "down", "bearish", "hands", "puts", "sell",
    //            "red", "done", "dead", "plummet", "loss", "upside down", "upsidedown" };
    //        var positiveScore = positiveWords.Count(word => text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
    //        var negativeScore = negativeWords.Count(word => text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

    //        foreach (var comment in post.Comments.ITop)
    //        {
    //            if (comment.Body != null)
    //            {
    //                positiveScore += positiveWords.Count(word => comment.Body.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
    //                negativeScore += negativeWords.Count(word => comment.Body.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);
    //            }
    //        }
    //        var sentiment = new List<string>();
    //        string sentimentType = "";

    //        if (positiveScore > negativeScore)
    //        {
    //            sentimentType = "Neutral";                
    //        }
    //        else if (negativeScore > positiveScore)
    //        {
    //            sentimentType = "Bad";
    //        }
    //        else
    //        {
    //            sentimentType = "Poor";
    //        }

    //        sentiment.Add($"{ticker}: ");
    //        sentiment.Add($"{sentimentType}");
    //        return sentiment;
    //    }
    }

    //public class TwitterSentimentAnalysis
    //{
    //    private static readonly HttpClient client = new HttpClient();

    //    public async Task<List<string>> AnalyzeTwitterAsync(List<string> tickers)
    //    {

    //        // Set your Twitter API v2 bearer token
    //        var bearerToken = "AAAAAAAAAAAAAAAAAAAAAEwxuQEAAAAAatn5FG9j3Levq%2BhY5dExYL9zSxI%3D2z3lo6kMeT0eJdtUh433zfO4lC74Od5K5yCJDUOOxebsqSsyq0";

    //        foreach (var ticker in tickers)
    //        {
    //            // Example: Fetch recent tweets containing a specific query
    //            var tweets = await FetchTweets("AAPL", bearerToken, 1);

    //            // Output the tweets
    //            foreach (var tweet in tweets)
    //            {
    //                Console.WriteLine($"{tweet.AuthorId}: {tweet.Text}");
    //            }
    //        }

    //        return new List<string>();
    //    }


    //    private static async Task<List<Tweet>> FetchTweets(string query, string bearerToken, int count)
    //    {
    //        try
    //        {
    //            // Twitter API v2 endpoint for searching tweets
    //            var url = $"https://api.twitter.com/2/tweets/search/recent?query={Uri.EscapeDataString(query)}&max_results={count}";

    //            // Set up HTTP request headers
    //            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

    //            // Send the GET request to Twitter API v2
    //            var response = await client.GetAsync(url);

    //            // Check if request was successful
    //            response.EnsureSuccessStatusCode();

    //            // Parse the JSON response
    //            var responseContent = await response.Content.ReadAsStringAsync();
    //            var tweetsResponse = JsonConvert.DeserializeObject<TweetsResponse>(responseContent);

    //            return tweetsResponse?.Data ?? new List<Tweet>();
    //        }
    //        catch (HttpRequestException ex)
    //        {
    //            Console.WriteLine($"Error fetching tweets: {ex.Message}");
    //            return new List<Tweet>();
    //        }
    //    }
    //}

    // Define classes to deserialize JSON response
    //public class Tweet
    //{
    //    public string Id { get; set; }
    //    public string AuthorId { get; set; }
    //    public string Text { get; set; }
    //}

    //public class TweetsResponse
    //{
    //    public List<Tweet> Data { get; set; }
    //}

    //public class StocktwitSentimentAnalysis
    //{
    //    public async Task<List<string>> AnalyzeStocktwitAsync()
    //    {
    //        // Simulate asynchronous Stocktwit sentiment analysis
    //        await Task.Delay(1000);
    //        return new List<string> { "Neutral", "Positive" }; // Simulated sentiments
    //    }
    //}
}
