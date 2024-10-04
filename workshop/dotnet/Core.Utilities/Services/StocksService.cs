using Ardalis.GuardClauses;
using Core.Utilities.Config;
using Core.Utilities.Models;
using System.Net.Http.Json;

namespace Core.Utilities.Services
{
    public class StocksService
    {
        private readonly HttpClient _httpClient;

        private string _apiKey;

        public StocksService(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("https://api.polygon.io/v1/");
            _httpClient = httpClient;
            _apiKey = AISettingsProvider.GetSettings().StockServiceApiKey;
        }
        public async Task<Stock> GetStockDailyOpenClose(string symbol, DateTime? date = null)
        {
            //Default to yesterday's date if no date is provided as current day pricing requires premium subscription
            string dateFormatted = date?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var requestUri = $"open-close/{symbol}/{dateFormatted}?adjusted=true&apiKey={_apiKey}";
            Stock stock = await GetHttpResponse<Stock>(requestUri);
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
