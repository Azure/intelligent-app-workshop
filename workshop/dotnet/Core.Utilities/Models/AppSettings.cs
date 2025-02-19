using System.Text.Json.Serialization;

namespace Core.Utilities.Models;

public record AppSettings (
    StockService StockService,
    AIFoundryProject AIFoundryProject,
    ManagedIdentity ManagedIdentity
);

public record StockService (
    string ApiKey
);

public record ManagedIdentity (
    string ClientId
);

public record AIFoundryProject (
    string ConnectionString,
    string GroundingWithBingConnectionId,
    string Endpoint,
    string DeploymentName,
    string ApiKey
);