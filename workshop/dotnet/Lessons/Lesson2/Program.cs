using Core.Utilities.Config;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// TODO: Step 1 - add ChatCompletion import

// Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
Kernel kernel = builder.Build();

// TODO: Step 2a - Get chatCompletionService and initialize chatHistory wiht system prompt

// TODO: Step 2b - Remove the promptExecutionSettings and kernelArgs initialization code
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    // Add Auto invoke kernel functions as the tool call behavior
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
        // TODO: Step 3 - Initialize fullMessage variable and add user input to chat history


        // TODO: Step 4 - Remove the foreach loop and replace it with `chatCompletionService` code 
        // including adding assistant message to chat history
        await foreach (var response in kernel.InvokePromptStreamingAsync(userInput, kernelArgs))
        {
            Console.Write(response);
        }

        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);
