using Microsoft.SemanticKernel;

namespace Core.Utilities.Config;

public static class KernelBuilderProvider
{
    public static IKernelBuilder CreateKernelWithChatCompletion()
    {
        var applicationSettings = AISettingsProvider.GetSettings();
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
                applicationSettings.OpenAI.DeploymentName,
                applicationSettings.OpenAI.Endpoint,
                applicationSettings.OpenAI.ApiKey);
        if (!string.IsNullOrEmpty(applicationSettings.OpenAI.EmbeddingsDeploymentName)) {
            #pragma warning disable SKEXP0010 // Suppress the specific warning
            builder.AddAzureOpenAITextEmbeddingGeneration(
                    applicationSettings.OpenAI.EmbeddingsDeploymentName,
                    applicationSettings.OpenAI.Endpoint,
                    applicationSettings.OpenAI.ApiKey);
            #pragma warning restore SKEXP0010 // Restore the warning
        }
        return builder;
    }
}