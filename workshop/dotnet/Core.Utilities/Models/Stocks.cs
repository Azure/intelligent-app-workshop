namespace Core.Utilities.Models
{
    public record Stocks(List<Stock> Stock);

    public record Stock(
        string Symbol,
        double Open,
        double Close,
        double AfterHours,
        double PreMarket,
        string From,
        string Status = "OK"
    );
}
