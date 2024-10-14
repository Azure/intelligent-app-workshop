namespace Core.Utilities.Models;

public record Stocks(List<Stock> Stock);

public record Stock(
    string Symbol,
    double Open,
    double Close,
    double High,
    double Low,
    string From,
    string Status = "OK"
);

