using Core.Utilities.Config;
// Add import for Plugins
using Core.Utilities.Plugins;
// Add import required for StockService
using Core.Utilities.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;

// TODO: Step 1 -- Add imports for Agents and Azure.Identity
using Microsoft.SemanticKernel.Agents.AzureAI;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel.Agents;


// TODO: Step 2 - Initialize connection to Grounding with Bing Search tool and agent
var foundryEndpoint = AISettingsProvider.GetSettings().AIFoundryProject.FoundryEndpoint;
var client3 = AzureAIAgent.CreateAgentsClient(foundryEndpoint, new AzureCliCredential());
PersistentAgent definition3 = await client3.Administration.CreateAgentAsync("gpt-4o");
        
Console.WriteLine($"Foundry Endpoint: {foundryEndpoint}");
var groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;

BingGroundingSearchConfiguration bingToolConfiguration = new(AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId);
BingGroundingSearchToolParameters bingToolParameters = new([bingToolConfiguration]);

var client = AzureAIAgent.CreateAgentsClient(foundryEndpoint, new AzureCliCredential());

PersistentAgent definition = await client.Administration.CreateAgentAsync(
    //AISettingsProvider.GetSettings().AIFoundryProject.DeploymentName,
    "gpt-4o",
    "StockSentimentAgent",
    "An agent that provides stock sentiment analysis and recommendations.",
    """
            Your responsibility is to find the stock sentiment for a given Stock.

            RULES:
            - Report a stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
            - Only use current data reputable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
            - Provide the stock sentiment scale in your response and a recommendation to buy, hold or sell.
            - Include the reasoning behind your recommendation.
            - Be sure to cite the source of the information.
            """
    //tools: [new BingGroundingToolDefinition(bingToolParameters)]
    );

AzureAIAgent agent = new(
    definition,
    client,
    templateFactory: new KernelPromptTemplateFactory());

agent.Kernel.Plugins.AddFromObject(new TimeInformationPlugin());

// Initialize Stock Data Plugin and register it in the kernel
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
agent.Kernel.Plugins.AddFromObject(stockDataPlugin);

// Get chatCompletionService and initialize chatHistory with system prompt
ChatHistory chatHistory = new("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
// Remove the promptExecutionSettings and kernelArgs initialization code
// Add system prompt
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    // Add Auto invoke kernel functions as the tool call behavior
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// TODO: Step 3 - Uncomment out all code after "Execute program" comment
// Execute program.

// Create a thread for the agent conversation.
AgentThread thread = new AzureAIAgentThread(client);

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
