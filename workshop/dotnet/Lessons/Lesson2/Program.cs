using Core.Utilities.Config;
// TODO: Step 3 - Add import required for StockService
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
Kernel kernel = builder.Build();

// TODO: Step 1 - Initialize Time plugin and registration in the kernel

// TODO: Step 4 - Initialize Stock Data Plugin and register it in the kernel

// Add system propmpt
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    // Step 2 - Add Auto invoke kernel functions as the tool call behavior
    ChatSystemPrompt = @"You are a friendly financial advisor that only emits financial advice in a creative and funny tone"
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
        // Provide kernel arguments as a second parameter
        await foreach (var response in kernel.InvokePromptStreamingAsync(userInput, kernelArgs))
        {
            Console.Write(response);
        }
        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);