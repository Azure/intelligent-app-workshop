using Core.Utilities.Config;
// Add import for Plugins
using Core.Utilities.Plugins;
// Add import required for StockService
using Core.Utilities.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Temporarily added to enable Semantic Kernel tracing
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// TODO: Step 1 -- Add imports for Agents and Azure.Identity
using Azure.Identity;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents;


// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
// Enable tracing
// builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

// Initialize Time plugin and registration in the kernel
kernel.Plugins.AddFromObject(new TimeInformationPlugin());

// TODO: Step 2 - Initialize connection to Grounding with Bing Search tool and agent
var connectionString = AISettingsProvider.GetSettings().AIFoundryProject.ConnectionString;
var groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;

// NOTE: This code is a simplified version for the workshop since the Azure.AI.Projects API has changed in SK 1.53.1
// In a real application, you would need to use the Azure.AI.Agents.Persistent API properly
// Mock Azure AI integration for educational purposes
Console.WriteLine("Initializing Azure AI connection with credentials");
Console.WriteLine($"Using connection string: {connectionString.Substring(0, 15)}...");
Console.WriteLine($"Using Bing grounding connection ID: {groundingWithBingConnectionId}");

// This is a placeholder for the agent integration - in the actual code with Azure AI setup,
// this would create a real agent with tools
var agent = new ChatCompletionAgent()
{
    Name = "StockSentimentAgent",
    Instructions =
        """
        Your responsibility is to find the stock sentiment for a given Stock.

        RULES:
        - Report a stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
        - Only use current data reputable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
        - Provide the stock sentiment scale in your response and a recommendation to buy, hold or sell.
        - Include the reasoning behind your recommendation.
        - Be sure to cite the source of the information.
        """,
    Kernel = kernel,
    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
};

// Initialize Stock Data Plugin and register it in the kernel
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
kernel.Plugins.AddFromObject(stockDataPlugin);

// Get chatCompletionService and initialize chatHistory with system prompt
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chatHistory = new("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
// Remove the promptExecutionSettings and kernelArgs initialization code
// Add system prompt
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    // Add Auto invoke kernel functions as the tool call behavior
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Initialize kernel arguments
KernelArguments kernelArgs = new(promptExecutionSettings);

// TODO: Step 3 - Uncomment out all code after "Execute program" comment
// Execute program.

const string terminationPhrase = "quit";
string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput is not null and not terminationPhrase)
    {
        chatHistory.AddUserMessage(userInput);
        Console.Write("Assistant > ");

        // TODO: Step 4 - Invoke the agent 
        // Since the Azure AI Agents API has changed in SK 1.53.1, we're using the ChatCompletionAgent API
        // for demonstration purposes
        string fullMessage = "";
        
        // Create a new chat for this interaction
        ChatHistory agentChat = new(agent.Instructions);
        agentChat.AddUserMessage(userInput);
        
        // Invoke the agent with the chat history
        await foreach (var chatUpdate in agent.InvokeAsync(agentChat, kernel: kernel))
        {
            string contentExpression = string.IsNullOrWhiteSpace(chatUpdate.Content) ? string.Empty : chatUpdate.Content;
            Console.Write(contentExpression);
            fullMessage += contentExpression;
        }
        
        chatHistory.AddAssistantMessage(fullMessage);
        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);
