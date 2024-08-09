using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StockBot
{
    public class StockPriceAnalysis
    {
        private static readonly string apiKey = "24IVLIEYN5EA4LF9";
        AlphaVantageClient client = new AlphaVantageClient(apiKey);

        public async Task<List<string>> FetchStocksPriceAsync(List<String> stocks)
        {
            List<string> tickers = new List<string>();

            foreach (string stock in stocks)
            {
                double? week = await client.GetWeekly(stock);
                double? month = await client.GetMonthly(stock);

                tickers.Add(stock + "Week Change: " + week + "Month Change: " + month);
            }

            return tickers;
        }

        public async Task<List<string>> FetchStocksAsync(List<String> stocks)
        {
            List<string> tickers = new List<string>();

            foreach (string stock in stocks)
            {
                double epsGrowth = await client.GetEpsGrowth(stock);
                double? debtEquityRatio = await client.GetDebtEquityRatio(stock);
                bool hasSmaCrossover = await client.HasSmaCrossover(stock);

                if (epsGrowth > 0 && debtEquityRatio < 0.1 && hasSmaCrossover)
                {
                    tickers.Add(stock);
                    Console.WriteLine($"{stock}: EPS Growth {epsGrowth:F2}%, Debt/Equity {debtEquityRatio:F2}");
                }
            }

            return tickers;
        }
    }

    public class AlphaVantageClient
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://www.alphavantage.co/query";

        public AlphaVantageClient(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient();
        }

        public async Task<double> GetWeekly(string symbol)
        {
            try
            {
                string url = $"{ApiBaseUrl}?function=TIME_SERIES_WEEKLY&symbol={symbol}&apikey={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var price = JsonConvert.DeserializeObject<WeeklyResponse>(response);

                if (price != null)
                {
                    // Calculate Weekly growth percentage
                    return price.test;
                }
                else
                {
                    throw new Exception("Error fetching weekly price");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weekly growth for {symbol}: {ex.Message}");
                return double.NaN;
            }
        }

        public async Task<double> GetMonthly(string symbol)
        {
            try
            {
                string url = $"{ApiBaseUrl}?function=TIME_SERIES_MONTHLY&symbol={symbol}&apikey={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var price = JsonConvert.DeserializeObject<MonthlyResponse>(response);

                if (price != null)
                {
                    // Calculate Monthly growth percentage
                    return price.test;
                }
                else
                {
                    throw new Exception("Error fetching monthly price");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching monthly growth for {symbol}: {ex.Message}");
                return double.NaN;
            }
        }

        public async Task<double> GetEpsGrowth(string symbol)
        {
            try
            {
                string url = $"{ApiBaseUrl}?function=EARNINGS&symbol={symbol}&apikey={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var earnings = JsonConvert.DeserializeObject<EarningsResponse>(response);

                if (earnings != null && earnings.AnnualEarnings != null && earnings.AnnualEarnings.Count > 1)
                {
                    double currentYearEps = earnings.AnnualEarnings[0].Eps;
                    double previousYearEps = earnings.AnnualEarnings[1].Eps;

                    // Calculate EPS growth percentage
                    return (currentYearEps - previousYearEps) / previousYearEps * 100;
                }
                else
                {
                    throw new Exception("Error fetching EPS growth data");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching EPS growth for {symbol}: {ex.Message}");
                return double.NaN;
            }
        }

        public async Task<double?> GetDebtEquityRatio(string symbol)
        {
            try
            {
                //EARNINGS
                string url = $"{ApiBaseUrl}?function=BALANCE_SHEET&symbol={symbol}&apikey={_apiKey}";
                var response = await _httpClient.GetStringAsync(url);
                var overview = JsonConvert.DeserializeObject<CompanyOverviewResponse>(response);

                if (overview != null && overview.DebtToEquity != null)
                {
                    long assets = Convert.ToInt64(overview.DebtToEquity[0].TotalAssets);
                    long debts = Convert.ToInt64(overview.DebtToEquity[0].CurrentDebt);
                    return (debts - assets) / assets * 100;
                }
                else
                {
                    throw new Exception("Debt/Equity ratio not available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Debt/Equity ratio for {symbol}: {ex.Message}");
                return double.NaN;
            }
        }

        public async Task<bool> HasSmaCrossover(string symbol)
        {
            try
            {
                string url20 = $"{ApiBaseUrl}?function=SMA&symbol={symbol}&interval=daily&time_period=20&series_type=close&apikey={_apiKey}";
                string url50 = $"{ApiBaseUrl}?function=SMA&symbol={symbol}&interval=daily&time_period=50&series_type=close&apikey={_apiKey}";

                var response20 = await _httpClient.GetStringAsync(url20);
                var response50 = await _httpClient.GetStringAsync(url50);

                var sma20 = JsonConvert.DeserializeObject<SmaResponse>(response20);
                var sma50 = JsonConvert.DeserializeObject<SmaResponse>(response50);

                if (sma20 != null && sma50 != null &&
                    sma20.Sma != null && sma50.Sma != null &&
                    sma20.Sma.Length > 1 && sma50.Sma.Length > 1)
                {
                    // Check if SMA 20 crossed above SMA 50
                    return sma20.Sma[0] > sma50.Sma[0] && sma20.Sma[1] <= sma50.Sma[1];
                }
                else
                {
                    throw new Exception("SMA data not available");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking SMA crossover for {symbol}: {ex.Message}");
                return false;
            }
        }

        // Helper classes to deserialize JSON responses
        private class EarningsResponse
        {
            [JsonProperty("annualEarnings")]
            public List<AnnualEarnings> AnnualEarnings { get; set; }
        }

        private class AnnualEarnings
        {
            [JsonProperty("fiscalDateEnding")]
            public DateTime FiscalDateEnding { get; set; }

            [JsonProperty("reportedEPS")]
            public double Eps { get; set; }
        }

        private class CompanyOverviewResponse
        {
            [JsonProperty("annualReports")]
            public List<DebtToEquity> DebtToEquity { get; set; }
        }

        private class DebtToEquity
        {
            [JsonProperty("totalAssets")]
            public string TotalAssets { get; set; }

            [JsonProperty("currentDebt")]
            public string CurrentDebt { get; set; }
        }

        private class SmaResponse
        {
            [JsonProperty("SMA")]
            public double[] Sma { get; set; }
        }

        private class WeeklyResponse
        {
            [JsonProperty("test")]
            public double test { get; set; }
        }

        private class MonthlyResponse
        {
            [JsonProperty("test")]
            public double test { get; set; }
        }
    }
}
