using Ardalis.GuardClauses;
using Core.Utilities.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Core.Utilities.Config;

internal static class AISettingsProvider
{
    internal static AISettings GetSettings()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var aiSettings = config
            .Get<AISettings>();

        Guard.Against.Null(aiSettings);
        Guard.Against.NullOrEmpty(aiSettings.ModelName);
        Guard.Against.NullOrEmpty(aiSettings.ApiKey);
        Guard.Against.NullOrEmpty(aiSettings.Endpoint);

        return aiSettings;
    }
}