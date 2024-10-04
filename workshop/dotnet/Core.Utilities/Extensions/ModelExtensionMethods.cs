using Core.Utilities.Models;
using System.Text;
using System.Text.Json;

namespace Core.Utilities.Extensions
{
    public static class ModelExtensionMethods
    {
        public static string FormatStockData(this Stock stockData)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("| Symbol | Price | Pre Market Price | After Hours Price | Date ");
            stringBuilder.AppendLine("| ----- | ----- | ----- | ----- |");
            stringBuilder.AppendLine($"| {stockData.Symbol} | {stockData.Open} | {stockData.PreMarket} | {stockData.AfterHours} | {stockData.From} ");

            return stringBuilder.ToString();
        }
    }
}
