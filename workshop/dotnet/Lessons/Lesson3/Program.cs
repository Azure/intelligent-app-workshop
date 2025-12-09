using Core.Utilities.Config;
using Core.Utilities.Plugins;
using Core.Utilities.Services;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;

// Initialize the chat client with Agent Framework  
IChatClient chatClient = AgentFrameworkProvider.CreateChatClientWithApiKey();

// Initialize plugins
TimeInformationPlugin timePlugin = new();
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));

// Create AI Functions from plugins
var tools = new AIFunction[]
{
    AIFunctionFactory.Create(timePlugin.GetCurrentUtcTime),
    AIFunctionFactory.Create(stockDataPlugin.GetStockPrice),
    AIFunctionFactory.Create(stockDataPlugin.GetStockPriceForDate)
};

// **TODO: Step 1** - Define specialized system instructions for stock sentiment analysis:

// **TODO: Step 2** - Create the specialized Stock Sentiment Agent:

// Create a thread for conversation
AgentThread thread = agent.GetNewThread();

// Execute program
const string terminationPhrase = "quit";
string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();
    
    // Handle null input (e.g., from piped input or EOF)
    if (userInput == null)
    {
        Console.WriteLine("Input ended. Exiting...");
        break;
    }

    if (userInput is not terminationPhrase)
    {
        Console.Write("Assistant > ");
        
        // Use agent with automatic function calling
        var response = await agent.RunAsync(userInput, thread);
        Console.WriteLine(response);
    }
}
while (userInput != terminationPhrase);