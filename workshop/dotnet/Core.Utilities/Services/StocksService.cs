using Ardalis.GuardClauses;
using Core.Utilities.Config;
using Core.Utilities.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Core.Utilities.Services
{
    /// <summary>
    /// Service for retrieving stock information from the Polygon API.
    /// </summary>
    public class StocksService
    {
        private readonly HttpClient _httpClient;

        private string _apiKey;

        private static ILogger<StocksService> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StocksService>();

        public StocksService(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.polygon.io/v1/");
            _httpClient = httpClient;
            _apiKey = AISettingsProvider.GetSettings().StockServiceApiKey;
        }

        /// <summary>
        /// Retrieves the daily open and close prices for a stock.
        /// NOTE: this requires a premium subscription to get data for the current month
        /// </summary>
        public async Task<Stock> GetStockDailyOpenClose(string symbol, DateTime? date = null)
        {
            //Default to yesterday's date if no date is provided as current day pricing requires premium subscription
            string dateFormatted = date?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var requestUri = $"open-close/{symbol}/{dateFormatted}?adjusted=true&apiKey={_apiKey}";
            Stock stock = await GetHttpResponse<Stock>(requestUri);
            return stock;
        }


        /// <summary>
        /// Retrieves the aggregate stock information for a given symbol and date.
        /// NOTE: using this to get stock last price as it is free to usecd 
        /// </summary>
        public async Task<Stock> GetStockAggregate(string symbol, DateTime? date = null) 
        {
            logger.LogDebug("> Getting stock aggregate");
            //Default to yesterday's date if no date is provided as current day pricing requires premium subscription
            DateTime toDate = date ?? DateTime.Now.AddDays(-1);
            string toDateFormatted = toDate.ToString("yyyy-MM-dd");
            //Default fromDate to 4 days ago to account for weekends and Monday holidays
            string fromDateFormatted = toDate.AddDays(-4).ToString("yyyy-MM-dd");
            var requestUri = $"/v2/aggs/ticker/{symbol}/range/1/day/{fromDateFormatted}/{toDateFormatted}?adjusted=true&sort=desc&apiKey=";
            // Log the requestUri without the apiKey
            logger.LogDebug($"Request URI: {requestUri}");
            requestUri += $"{_apiKey}";

            AggregateStockResponse response = await GetHttpResponse<AggregateStockResponse>(requestUri);
            Stock stock = null;
            if (response.StockResults.Count > 0) {
                StockResult stockResult = response.StockResults[0];
                stock = new Stock(
                    Symbol: symbol,
                    Open: stockResult.Open,
                    High: stockResult.High,
                    Low: stockResult.Low,
                    Close: stockResult.Close,
                    From: toDateFormatted,
                    Status: response.Status
                );

            }
            logger.LogDebug("< Getting stock aggregate count: " + response.ResultsCount);
            return stock;
        }


        private async Task<T> GetHttpResponse<T>(string requestUri)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode}, message: {errorMessage}");
            }

            T? data = await response.Content.ReadFromJsonAsync<T>();
            Guard.Against.Null(data);

            return data;
        }
    }
}
