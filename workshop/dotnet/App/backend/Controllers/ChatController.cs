using Core.Utilities.Models;
using Core.Utilities.Extensions;
// Add import required for StockService
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Add import for Agents
using Microsoft.SemanticKernel.Agents;
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

    private readonly ChatCompletionAgent _stockSentimentAgent;
    public ChatController(Kernel kernel)
    {
        _kernel = kernel;
        _promptExecutionSettings = new()
        {
            // Add Auto invoke kernel functions as the tool call behavior
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };
        _stockSentimentAgent = GetStockSemanticAgentAsync().Result;

    }

    /// <summary>
    /// Get StockSemanticAgent instance
    /// </summary>
    /// <returns></returns>
    private async Task<ChatCompletionAgent> GetStockSemanticAgentAsync()
    {
        // Get StockSemanticAgent instance
        ChatCompletionAgent stockSentimentAgent =
            new()
            {
                Name = "StockSentimentAgent",
                Instructions =
                    """
                    Your responsibility is to find the stock sentiment for a given Stock.

                    RULES:
                    - Use stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
                    - Only use reliable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
                    - Provide the rating in your response and a recommendation to buy, hold or sell.
                    - Include the reasoning behind your recommendation.
                    - Include the source of the sentiment in your response.
                    """,
                Kernel = _kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { 
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
            };
        return stockSentimentAgent;
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
        KernelArguments kernelArgs = new(_promptExecutionSettings);

        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        if (request.InputMessage != null)
        {
            chatHistory.AddUserMessage(request.InputMessage);

            // Invoke stockSentimentAgent chat completion with kernel arguments
            await foreach (var chatUpdate in _stockSentimentAgent.InvokeAsync(chatHistory, kernelArgs))
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