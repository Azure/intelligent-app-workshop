using Ardalis.GuardClauses;
using Core.Utilities.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Core.Utilities.Config;

internal static class AISettingsProvider
{
    internal static AppSettings GetSettings()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var aiSettings = config
            .Get<AppSettings>();
        Guard.Against.Null(aiSettings);
        Guard.Against.Null(aiSettings.OpenAI);
        Guard.Against.NullOrEmpty(aiSettings.OpenAI.ModelName);
        Guard.Against.NullOrEmpty(aiSettings.OpenAI.ApiKey);
        Guard.Against.NullOrEmpty(aiSettings.OpenAI.Endpoint);

        return aiSettings;
    }
}