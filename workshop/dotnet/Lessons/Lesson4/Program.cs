using Core.Utilities.Config;
using Core.Utilities.Plugins;
using Core.Utilities.Services;
using Core.Utilities.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Azure.Identity;
using Azure.AI.Agents.Persistent;
using Azure.AI.OpenAI;

// Get AI Foundry settings
var applicationSettings = AISettingsProvider.GetSettings();

// **TODO** Step 1 - Add Azure Foundry environment variables

// **TODO** Step 2 - Create a persistent client

// Initialize plugins  
TimeInformationPlugin timePlugin = new();
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));

// **TODO** Step 3 - Create web search tool for Bing grounding (requires BING_CONNECTION_ID environment variable)

// Create AI Functions from plugins
var timeTool = AIFunctionFactory.Create(timePlugin.GetCurrentUtcTime);
var stockPriceTool = AIFunctionFactory.Create(stockDataPlugin.GetStockPrice);
var stockPriceDateTool = AIFunctionFactory.Create(stockDataPlugin.GetStockPriceForDate);

// ** TODO ** Step 4 - Stock Sentiment Agent system instructions and initialization - defines the agent's behavior and rules
HostedWebSearchTool webSearchTool = new();

// Execute program
const string terminationPhrase = "quit";
string? userInput;

Console.WriteLine("=== Financial Analysis Agent with Microsoft Agent Framework ===");
Console.WriteLine("This agent provides comprehensive financial analysis using web search and market data.");
Console.WriteLine("Ask any financial question - about specific stocks, market trends, sectors, or investment strategies.");
Console.WriteLine("Examples: 'What do you think about Microsoft?', 'How is the tech sector performing?', 'Should I invest in renewable energy stocks?'");
Console.WriteLine("Type 'quit' to exit.");
Console.WriteLine("==================================================================================");
Console.WriteLine();

do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput is not null and not terminationPhrase)
    {
        Console.Write("Assistant > ");
        
        try
        {
            // Use the agent to process the user message with local tools (following GitHub example)
            var response = await agent.RunAsync(userInput, thread, agentOptions);
            Console.WriteLine(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);

Console.WriteLine("Thank you for using the Financial Analysis Agent!");


