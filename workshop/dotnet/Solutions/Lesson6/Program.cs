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
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.SemanticKernel.Agents.AzureAI;


// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
// Enable tracing
builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

// Initialize Time plugin and registration in the kernel
kernel.Plugins.AddFromObject(new TimeInformationPlugin());

// TODO: Step 2 - Initialize connection to Grounding with Bing Search tool and agent
var connectionString = AISettingsProvider.GetSettings().AIFoundryProject.ConnectionString;
var groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;

var projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());

ConnectionResponse bingConnection = await projectClient.GetConnectionsClient().GetConnectionAsync(groundingWithBingConnectionId);
var connectionId = bingConnection.Id;

ToolConnectionList connectionList = new ToolConnectionList
{
    ConnectionList = { new ToolConnection(connectionId) }
};
BingGroundingToolDefinition bingGroundingTool = new BingGroundingToolDefinition(connectionList);

var clientProvider =  AzureAIClientProvider.FromConnectionString(connectionString, credentials);
AgentsClient client = clientProvider.Client.GetAgentsClient();
var definition = await client.CreateAgentAsync(
    "gpt-4o",
    instructions:
            """
            Your responsibility is to find the stock sentiment for a given Stock.

            RULES:
            - Report a stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
            - Only use current data reputable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
            - Provide the stock sentiment scale in your response and a recommendation to buy, hold or sell.
            - Include the reasoning behind your recommendation.
            - Be sure to cite the source of the information.
            """,
    tools:
    [
        bingGroundingTool
    ]);
var agent = new AzureAIAgent(definition, clientProvider)
{
    Kernel = kernel,
};

// Create a thread for the agent conversation.
AgentThread thread = await client.CreateThreadAsync();

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
        ChatMessageContent message = new(AuthorRole.User, userInput);
        await agent.AddChatMessageAsync(thread.Id, message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(thread.Id))
        {
            string contentExpression = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : response.Content;
            chatHistory.AddAssistantMessage(contentExpression);
            Console.WriteLine($"{contentExpression}");
        }
    }
}
while (userInput != terminationPhrase);
