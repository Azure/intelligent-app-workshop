using Core.Utilities.Services;
using Core.Utilities.Extensions;

using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Core.Utilities.Plugins;

public class StockDataPlugin(StocksService stockService)
{
    private readonly StocksService _stockService = stockService;

    [KernelFunction, Description("Gets stock price")]
    public async Task<string> GetStockPrice(string symbol)
    {
        string tabularData = (await _stockService.GetStockAggregate(symbol)).FormatStockData();
        return tabularData;
    }

    [KernelFunction, Description("Gets stock price for a given date")]
    public async Task<string> GetStockPriceForDate(string symbol, string date)
    {
        var dateTime = DateTime.Parse(date);
        string tabularData = (await _stockService.GetStockAggregate(symbol, dateTime)).FormatStockData();
        return tabularData;
    }

}