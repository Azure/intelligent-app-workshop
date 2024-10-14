using Core.Utilities.Services;
using Core.Utilities.Models;
using Core.Utilities.Extensions;

using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Plugins;

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
    public async Task<string> GetStockPriceForDate(string symbol, DateTime date)
    {
        string tabularData = (await _stockService.GetStockAggregate(symbol, date)).FormatStockData();
        return tabularData;
    }

}