using Core.Utilities.Config;
using Core.Utilities.Models;
// Add import for Plugins
using Plugins;
// Add import required for StockService
using Core.Utilities.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Temporarily added to enable Semantic Kernel tracing
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase {

    private readonly Kernel _kernel;

    public ChatController()
    {
        IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
        // Enable tracing
        builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
        _kernel = builder.Build();

        // Step 2 - Initialize Time plugin and registration in the kernel
        _kernel.Plugins.AddFromObject(new TimeInformationPlugin());

        // Step 6 - Initialize Stock Data Plugin and register it in the kernel
        HttpClient httpClient = new();
        StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
        _kernel.Plugins.AddFromObject(stockDataPlugin);
    }

    [HttpPost("/")]
    public async Task<String> ReplyAsync(
        String userInput, String[] history)
    {
        // Get chatCompletionService and initialize chatHistory wiht system prompt
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = new(string.Join("\n", history));
        chatHistory.AddUserMessage(userInput);
        chatHistory.AddSystemMessage("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
        // Add system prompt
        OpenAIPromptExecutionSettings promptExecutionSettings = new()
        {
            // Step 3 - Add Auto invoke kernel functions as the tool call behavior
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        // Initialize kernel arguments
        KernelArguments kernelArgs = new(promptExecutionSettings);

        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        if (userInput != null)
        {
            chatHistory.AddUserMessage(userInput);

            // Step 4 - Provide promptExecutionSettings and kernel arguments
            await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, _kernel))
            {
                Console.Write(chatUpdate.Content);
                fullMessage += chatUpdate.Content ?? "";
            }
            chatHistory.AddAssistantMessage(fullMessage);
        }

        return fullMessage;
    }


}