using System.Text.Json.Serialization;

namespace Core.Utilities.Models;

public class AppSettings {

    public string Endpoint { get; set; } = string.Empty;
    
    public string ModelName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string StockServiceApiKey { get; set;} = string.Empty;
    
};