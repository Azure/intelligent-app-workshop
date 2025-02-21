using Ardalis.GuardClauses;
using Core.Utilities.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Core.Utilities.Config;

public static class AISettingsProvider
{
    public static AppSettings GetSettings()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var aiSettings = config
            .Get<AppSettings>();
        Guard.Against.Null(aiSettings);
        Guard.Against.Null(aiSettings.AIFoundryProject);
        Guard.Against.NullOrEmpty(aiSettings.AIFoundryProject.DeploymentName);
        Guard.Against.NullOrEmpty(aiSettings.AIFoundryProject.GroundingWithBingConnectionId);
        Guard.Against.NullOrEmpty(aiSettings.AIFoundryProject.ApiKey);
        Guard.Against.NullOrEmpty(aiSettings.AIFoundryProject.Endpoint);
        Guard.Against.NullOrEmpty(aiSettings.AIFoundryProject.ConnectionString);

        return aiSettings;
    }
}