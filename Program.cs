using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StockBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Stock Trading Application");

            //// Initialize Webull API
            //WebullApi webullApi = new WebullApi();

            //// Login to Webull Account
            //bool loginSuccess = await webullApi.LoginAsync("username", "password");
            //if (!loginSuccess)
            //{
            //    Console.WriteLine("Login failed. Exiting...");
            //    return;
            //}
            //Console.WriteLine("Login successful.");

            //// Get Account Buying Power
            //int buyingPower = await webullApi.GetBuyingPowerAsync();
            //Console.WriteLine($"Buying Power: {buyingPower}");

            //// Check Day Trades Available
            //bool dayTradesAvailable = await webullApi.AreDayTradesAvailableAsync();
            //Console.WriteLine($"Day Trades Available: {dayTradesAvailable}");

            //// Get Current Stocks
            //List<string> currentStocks = await webullApi.GetCurrentStocksAsync();
            //Console.WriteLine("Current Stocks: " + string.Join(", ", currentStocks));

            //// Perform Sentiment Analysis
            RedditSentimentAnalysis sentimentAnalysisA = new RedditSentimentAnalysis();
            List<string> redditSentiments = await sentimentAnalysisA.AnalyzeRedditAsync();
            Console.WriteLine(redditSentiments);
            List<string> solidRedditSentiments = await HasDuplicateItemsBeforeColon(redditSentiments);

            List<string> tickers = new List<string>();
            foreach (string item in solidRedditSentiments)
            {
                tickers.Add(await GetStringBeforeColon(item));
            }
            Console.WriteLine("Reddit analysis returned " + tickers.Count + " tickers: ");        
            Console.WriteLine(tickers.ToString());
            //TwitterSentimentAnalysis sentimentAnalysisB = new TwitterSentimentAnalysis();
            //List<string> twitterSentiments = await sentimentAnalysisB.AnalyzeTwitterAsync(tickers);

            //StocktwitSentimentAnalysis sentimentAnalysisC = new StocktwitSentimentAnalysis();
            //List<string> stocktwitSentiments = await sentimentAnalysisC.AnalyzeStocktwitAsync();

            //Console.WriteLine("Twitter Sentiments: " + string.Join(", ", twitterSentiments));
            //Console.WriteLine("Stocktwit Sentiments: " + string.Join(", ", stocktwitSentiments));

            //// Perform Stock Price Analysis
            //StockPriceAnalysis stockPriceAnalysis = new StockPriceAnalysis();
            //List<string> stockScreener = await stockPriceAnalysis.FetchStocksAsync(tickers);
            List<string> tickersAnalysis = new List<string>();
            foreach (string ticker in tickers)
            {
                tickersAnalysis.Add(ticker);
            }           
            StockPriceAnalysis stockPriceAnalysis = new StockPriceAnalysis();
            List<string> priceChange = await stockPriceAnalysis.FetchStocksPriceAsync(tickersAnalysis);
            Console.WriteLine("The price action for each of the " + priceChange.Count + " tickers is: ");
            Console.WriteLine(priceChange);

            StockPriceAnalysis crossOverList = new StockPriceAnalysis();
            List<string> crossOver = await stockPriceAnalysis.FetchStocksAsync(tickersAnalysis);
            Console.WriteLine("The stocks with an SMA crossover indicater " + priceChange.Count + " tickers: ");
            Console.WriteLine(priceChange);


            //foreach (string stock in stockScreener)
            //{
            //    foreach (string symbol in tickers)
            //    {
            //        if (stock == symbol)
            //        {
            //            potentialBuys.Add(stock);
            //        }
            //    }
            //}
            //Console.WriteLine("The final results returned " + tickers.Count + " tickers: ");
            //Console.WriteLine(potentialBuys.ToString());
            //// Add logic for stock price analysis here

            //// Begin Stock Transaction
            //bool transactionInitiated = await webullApi.BeginStockTransactionAsync("AAPL", 10);
            //if (!transactionInitiated)
            //{
            //    Console.WriteLine("Stock transaction initiation failed. Exiting...");
            //    return;
            //}

            //// Run AwaitPurchaseConfirmation and SendPurchaseEmail asynchronously
            //var purchaseTask = webullApi.AwaitPurchaseConfirmationAsync("AAPL", 10);
            //var sendEmailTask = purchaseTask.ContinueWith(async t =>
            //{
            //    if (await t)
            //    {
            //        Console.WriteLine("Stock purchase confirmed.");
            //        EmailService emailService = new EmailService();
            //        bool emailSent = await emailService.SendPurchaseEmailAsync("AAPL", 10);
            //        if (emailSent)
            //        {
            //            Console.WriteLine("Purchase confirmation email sent.");
            //        }
            //        else
            //        {
            //            Console.WriteLine("Failed to send purchase confirmation email.");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("Stock purchase not confirmed.");
            //    }
            //});

            //await Task.WhenAll(purchaseTask, sendEmailTask);

            Console.WriteLine("Ending Stock Trading Application");
        }

        public static async Task<List<string>> HasDuplicateItemsBeforeColon(List<string> items)
        {
            Dictionary<string, int> itemCounts = new Dictionary<string, int>();
            List<string> sentiments = new List<string>();

            foreach (string item in items)
            {
                string key = await GetStringBeforeColon(item);

                if (itemCounts.ContainsKey(key))
                {
                    itemCounts[key]++;
                }
                else
                {
                    itemCounts[key] = 1;
                }
            }

            foreach (var kvp in itemCounts)
            {
                if (kvp.Value > 1)
                {
                    sentiments.Add($"{kvp.Key}: Solid");
                }
            }

            return sentiments;
        }

        public static async Task<string> GetStringBeforeColon(string input)
        {
            int colonIndex = input.IndexOf(':');
            if (colonIndex == -1)
            {
                return input;
            }
            return input.Substring(0, colonIndex).Trim();
        }
    }
}
