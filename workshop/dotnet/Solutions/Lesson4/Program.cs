using Core.Utilities.Config;
using Core.Utilities.Plugins;
using Core.Utilities.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Temporarily added to enable Semantic Kernel tracing
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


// Step 1 - Remove line of code to initialize kernel
//IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
// Enable tracing
//builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
// Step 2 - Remove line of code to build kernel
//Kernel kernel = builder.Build();
// Step 3 - Add code to get Kernel instance with Cache passing in memory store
Kernel kernel = KernelWithCacheProvider.GetKernelWithCache(services => { services.AddInMemoryVectorStore(); });

kernel.Plugins.AddFromObject(new TimeInformationPlugin());

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
        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        chatHistory.AddUserMessage(userInput);

        // Provide promptExecutionSettings and kernel arguments
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