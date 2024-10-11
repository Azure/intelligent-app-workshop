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
[Route("sk")]
public class ChatController : ControllerBase {

    private readonly Kernel _kernel;

    public ChatController(Kernel kernel)
    {
        _kernel = kernel;
    }

    [HttpPost("/chat")]
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