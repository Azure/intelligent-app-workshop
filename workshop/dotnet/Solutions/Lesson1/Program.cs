using Core.Utilities.Config;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


// TODO: Step 1 - Initialize the kernel with chat completion
IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
Kernel kernel = builder.Build();

// TODO: Step 2 - Add system propmpt
OpenAIPromptExecutionSettings promptExecutionSettings = new()
{
    ChatSystemPrompt = @"You are a friendly financial advisor that only emits financial advice in a creative and funny tone"
};

// TODO: Step 3 - Initialize kernel arguments
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
        // TODO: Step 4 - Provide kernel arguments as a second parameter
        await foreach (var response in kernel.InvokePromptStreamingAsync(userInput, kernelArgs))
        {
            Console.Write(response);
        }
        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);