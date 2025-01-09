using System.Text.Json.Serialization;

namespace Core.Utilities.Models;

public record AppSettings (
    OpenAI OpenAI,
    StockService StockService,
    BingSearchService BingSearchService
);

public record OpenAI (
    string Endpoint,
    string DeploymentName,
    string ApiKey
);

public record StockService (
    string ApiKey
);

public record BingSearchService (
    string ApiKey
);