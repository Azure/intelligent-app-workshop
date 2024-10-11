using Core.Utilities.Models;
using Core.Utilities.Extensions;
// Add import for Plugins
using Plugins;
// Add import required for StockService
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
    private readonly OpenAIPromptExecutionSettings _promptExecutionSettings;

    public ChatController(Kernel kernel)
    {
        _kernel = kernel;
        _promptExecutionSettings = new()
        {
            // Step 3 - Add Auto invoke kernel functions as the tool call behavior
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

    }

    [HttpPost("/chat")]
    public async Task<ChatResponse> ReplyAsync([FromBody]ChatRequest request)
    {
        // Get chatCompletionService and initialize chatHistory wiht system prompt
        var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        if (request.MessageHistory.Count == 0) { 
            chatHistory.AddSystemMessage("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
        }
        else {
            chatHistory = request.ToChatHistory();
        }

        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        if (request.InputMessage != null)
        {
            chatHistory.AddUserMessage(request.InputMessage);

            // Step 4 - Provide promptExecutionSettings and kernel arguments
            await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, _promptExecutionSettings, _kernel))
            {
                Console.Write(chatUpdate.Content);
                fullMessage += chatUpdate.Content ?? "";
            }
            chatHistory.AddAssistantMessage(fullMessage);
        }
        var chatResponse = new ChatResponse(fullMessage, chatHistory.FromChatHistory());    
        return chatResponse;
    }


}