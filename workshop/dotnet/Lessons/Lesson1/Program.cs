using Core.Utilities.Config;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;


// TODO: Step 1 - Initialize the kernel with chat completion


// TODO: Step 2 - Add system prompt


// TODO: Step 3 - Initialize kernel arguments

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
        // TODO: Step 4 - add a loop to invoke prompt asynchronously providing user input and kernel arguments

        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);