using System.Text.Json.Serialization;

namespace Core.Utilities.Models;

public record AggregateStockResponse(
    int ResultsCount,
    string Status,
    [property: JsonPropertyName("results")]
    List<StockResult> StockResults
);

public record StockResult(
    [property: JsonPropertyName("o")]
    double Open,
    [property: JsonPropertyName("c")]
    double Close,
    [property: JsonPropertyName("h")]
    double High,
    [property: JsonPropertyName("l")]
    double Low
);