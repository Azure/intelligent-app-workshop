using Core.Utilities.Services;
using Core.Utilities.Models;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Xunit;

namespace Core.Utilities.Services.Tests
{
    public class StocksServiceTest
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly StocksService _stocksService;

        public StocksServiceTest()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.polygon.io/v1/")
            };
            _stocksService = new StocksService(_httpClient);
        }

        [Fact]
        public async Task GetStockDailyOpenClose_ReturnsStock_WhenResponseIsSuccessful()
        {
            // Arrange
            string symbol = "AAPL";
            var date = new DateTime(2023, 10, 1);
            var stock = new Stock(symbol, 150.0, 155.0, 160.0, 145.0, DateTime.Now.ToString("yyyy-MM-dd"));

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(stock)
                });

            // Act
            var result = await _stocksService.GetStockDailyOpenClose(symbol, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(150.0, result.Open);
            Assert.Equal(155.0, result.Close);
        }

        [Fact]
        public async Task GetStockDailyOpenClose_ThrowsHttpRequestException_WhenResponseIsUnsuccessful()
        {
            // Arrange
            var symbol = "AAPL";
            var date = new DateTime(2023, 10, 1);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request")
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _stocksService.GetStockDailyOpenClose(symbol, date));
        }

        [Fact]
        public async Task GetStockDailyOpenClose_ReturnsStock_WhenResponseIsSuccessfulWitMockJson()
        {
            // Arrange
            string symbol = "MSFT";
            var date = new DateTime(2024, 10, 3);
            var jsonResponse = @"
            {
                ""status"": ""OK"",
                ""from"": ""2024-10-03"",
                ""symbol"": ""MSFT"",
                ""open"": 417.63,
                ""high"": 419.55,
                ""low"": 414.29,
                ""close"": 416.54,
                ""volume"": 13031361,
                ""afterHours"": 416.33,
                ""preMarket"": 416.91
            }";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _stocksService.GetStockDailyOpenClose(symbol, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MSFT", result.Symbol);
            Assert.Equal(417.63, result.Open);
            Assert.Equal(416.54, result.Close);
            Assert.Equal("2024-10-03", result.From);
            Assert.Equal("OK", result.Status);
        }


        [Fact]
        public async Task GetStockAggregate_ReturnsStock_WhenResponseIsSuccessfulWitMockJson()
        {
            // Arrange
            string symbol = "MSFT";
            var date = new DateTime(2024, 10, 3);
            var jsonResponse = @"
            {
                ""status"": ""OK"",
                ""ticker"": ""MSFT"",
                ""queryCount"": 1,
                ""results"": [
                    {
                        ""o"": 150.0,
                        ""h"": 155.50,
                        ""l"": 145.20,
                        ""c"": 152.54
                    }
                ]
            }";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _stocksService.GetStockAggregate(symbol, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(150.00, result.Open);
            Assert.Equal(155.50, result.High);
            Assert.Equal(145.20, result.Low);
            Assert.Equal(152.54, result.Close);
            Assert.Equal("OK", result.Status);
        }

        [Fact]
        public async Task GetStockAggregate_ReturnsStock_WhenResponseIsSuccessful()
        {
            // Arrange
            string symbol = "AAPL";
            var date = new DateTime(2023, 10, 1);

            var aggregateStockResponse = new AggregateStockResponse(
                1,
                "OK",
                new List<StockResult>
                {
                    new StockResult(Open: 150.0, High: 155.0, Low: 145.0, Close: 152.0)
                }
            );

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(aggregateStockResponse)
                });

            // Act
            var result = await _stocksService.GetStockAggregate(symbol, date);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(150.0, result.Open);
            Assert.Equal(155.0, result.High);
            Assert.Equal(145.0, result.Low);
            Assert.Equal(152.0, result.Close);
            Assert.Equal("OK", result.Status);
        }

        [Fact]
        public async Task GetStockAggregate_ThrowsHttpRequestException_WhenResponseIsUnsuccessful()
        {
            // Arrange
            var symbol = "AAPL";
            var date = new DateTime(2023, 10, 1);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad Request")
                });

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _stocksService.GetStockAggregate(symbol, date));
        }

        [Fact]
        public async Task GetStockAggregate_ReturnsNull_WhenNoStockResults()
        {
            // Arrange
            string symbol = "AAPL";
            var date = new DateTime(2023, 10, 1);
            var aggregateStockResponse = new AggregateStockResponse(
                0,
                "OK",
                new List<StockResult>()
            );
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(aggregateStockResponse)
                });

            // Act
            var result = await _stocksService.GetStockAggregate(symbol, date);

            // Assert
            Assert.Null(result);
        }
    }
}
