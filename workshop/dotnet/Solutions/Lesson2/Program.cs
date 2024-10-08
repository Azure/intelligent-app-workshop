using Core.Utilities.Config;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
// Step 1 - Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;


// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
Kernel kernel = builder.Build();

// Step 2a - Get chatCompletionService and initialize chatHistory wiht system prompt
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory chatHistory = new("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
// Step 2b - Remove the promptExecutionSettings and kernelArgs initialization code - REMOVED

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
        // Step 3 - Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        chatHistory.AddUserMessage(userInput);

        // Step 4 - Replace the foreach loop and replace it with this code including adding assistant message to chat history
        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            Console.Write(chatUpdate.Content);
            fullMessage += chatUpdate.Content ?? "";
        }
        chatHistory.AddAssistantMessage(fullMessage);

        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);