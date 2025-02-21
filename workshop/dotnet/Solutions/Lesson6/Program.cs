using Core.Utilities.Config;
// Add import for Plugins
using Core.Utilities.Services;
using Core.Utilities.Plugins;
using Microsoft.SemanticKernel;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Temporarily added to enable Semantic Kernel tracing
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Initialize the kernel with chat completion
var builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
kernel.Plugins.AddFromObject(stockDataPlugin);

// Initialize Time plugin and registration in the kernel
kernel.Plugins.AddFromObject(new TimeInformationPlugin());

// Initialize Bing Search plugin
var connectionString = AISettingsProvider.GetSettings().AIFoundryProject.ConnectionString;
var groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;
var credentials = new DefaultAzureCredential();
var projectClient = new AIProjectClient(connectionString, credentials);

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

// Execute program.
const string terminationPhrase = "quit";
string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput != null && userInput != terminationPhrase)
    {
        Console.Write("Assistant > ");

        ChatMessageContent message = new(AuthorRole.User, userInput);
        await agent.AddChatMessageAsync(thread.Id, message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(thread.Id))
        {
            // Include TextContent (via ChatMessageContent.Content), if present.
            string contentExpression = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : response.Content;
            Console.WriteLine($"{contentExpression}");
        }
    }
}
while (userInput != terminationPhrase);