using Microsoft.SemanticKernel;

namespace Core.Utilities.Config;

public static class KernelBuilderProvider
{
    public static IKernelBuilder CreateKernelWithChatCompletion()
    {
        var applicationSettings = AISettingsProvider.GetSettings();
        return Kernel
            .CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                applicationSettings.AIFoundryProject.DeploymentName,
                applicationSettings.AIFoundryProject.FoundryEndpoint,
                applicationSettings.AIFoundryProject.ApiKey);
    }
}