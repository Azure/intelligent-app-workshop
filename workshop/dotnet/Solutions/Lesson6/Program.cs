using Core.Utilities.Config;
// Add import for Plugins
using Core.Utilities.Plugins;
// Add import required for StockService
using Core.Utilities.Services;
// Add import for ModelExtensionMethods
using Core.Utilities.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// Step 1 - Add import for Agents
using Microsoft.SemanticKernel.Agents;

// Add imports for Bing Search plugin
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Temporarily added to enable Semantic Kernel tracing
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
// Enable tracing
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = builder.Build();

// Initialize Time plugin and registration in the kernel
kernel.Plugins.AddFromObject(new TimeInformationPlugin());

// Initialize Stock Data Plugin and register it in the kernel
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
kernel.Plugins.AddFromObject(stockDataPlugin);

// Initialize Bing Search plugin
var bingApiKey = AISettingsProvider.GetSettings().BingSearchService.ApiKey;
if (!string.IsNullOrEmpty(bingApiKey))
{
    var bingConnector = new BingConnector(bingApiKey);
    var bing = new WebSearchEnginePlugin(bingConnector);
    kernel.ImportPluginFromObject(bing, "bing");
}

// Get chatCompletionService and initialize chatHistory with system prompt
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
// Step 2 - Remove initial prompt
ChatHistory chatHistory = new();
// Remove the promptExecutionSettings and kernelArgs initialization code
// Add system prompt
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    // Add Auto invoke kernel functions as the tool call behavior
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

// Initialize kernel arguments
KernelArguments kernelArgs = new(promptExecutionSettings);

// Add call to print all plugins and functions
var functions = kernel.Plugins.GetFunctionsMetadata();
// Step 3 - Comment out line to print plugins
//Console.WriteLine(functions.ToPrintableString());

// Step 4 - Add code to create Stock Sentiment Agent
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
        Kernel = kernel,
        Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { 
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
    };

// Execute program.
// Step 5 - Uncomment previously commented code
const string terminationPhrase = "quit";
string? userInput;
do
{
    Console.Write("User > ");
    userInput = Console.ReadLine();

    if (userInput != null && userInput != terminationPhrase)
    {
        Console.Write("Assistant > ");
        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        chatHistory.AddUserMessage(userInput);

        // Provide promptExecutionSettings and kernel arguments
        // Step 6 - Replace chatCompletionService with stockSentimentAgent
        await foreach (var chatUpdate in stockSentimentAgent.InvokeAsync(chatHistory, kernelArgs))
        {
            Console.Write(chatUpdate.Content);
            fullMessage += chatUpdate.Content ?? "";
        }
        chatHistory.AddAssistantMessage(fullMessage);

        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);
