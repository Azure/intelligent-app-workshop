using System.Text.Json.Serialization;

namespace Core.Utilities.Models;

public record AppSettings (
    StockService StockService,
    AIFoundryProject AIFoundryProject,
    ManagedIdentity? ManagedIdentity = null // Not needed when running locally
);

public record StockService (
    string ApiKey
);

public record ManagedIdentity (
    string ClientId = "" // Not needed when running locally
);

public record AIFoundryProject (
    string FoundryEndpoint,
    string GroundingWithBingConnectionId,
    string DeploymentName,
    string ApiKey
);