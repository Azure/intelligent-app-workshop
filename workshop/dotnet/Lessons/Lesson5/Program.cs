using Core.Utilities.Config;
// Add import for Plugins
using Core.Utilities.Plugins;
// Add import required for StockService
using Core.Utilities.Services;
// Add import for ModelExtensionMethods
using Core.Utilities.Extensions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// TODO: Step 1 - Add import for Agents

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

// TODO: Step 2 - Add code to create Stock Sentiment Agent

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

// Add call to print all plugins and functions
var functions = kernel.Plugins.GetFunctionsMetadata();
// TODO: Step 0a - Comment line to print all plugins and functions
Console.WriteLine(functions.ToPrintableString());

// TODO: Step 0b - Uncomment out all code after "Execute program" comment
// Execute program.
/*
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

        // TODO: Step 3 - Replace chatCompletionService with stockSentimentAgent
        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel))
        {
            Console.Write(chatUpdate.Content);
            fullMessage += chatUpdate.Content ?? "";
        }
        chatHistory.AddAssistantMessage(fullMessage);

        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);
*/