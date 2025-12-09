using Core.Utilities.Config;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;

// TODO: Step 1 - Initialize the chat client using Microsoft Agent Framework

// TODO: Step 2 - Configure system message using Agent Framework pattern

// TODO: Step 3 - Create a ChatClientAgent with the chat client and system instructions

// TODO: Step 4 - Create a thread for conversation

// Execute program.
Console.WriteLine("=== Financial Advisor with Conversation Memory ===");
Console.WriteLine("Agent Framework automatically maintains conversation history through AgentThread.");
Console.WriteLine("Try asking follow-up questions to see the agent remember previous context!");
Console.WriteLine("Example: Ask 'What should I invest in?' then 'Tell me more about that recommendation'");
Console.WriteLine("Type 'quit' to exit.");
Console.WriteLine();

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
        
        // TODO: Step 5 - Use agent to respond to user input


    }
}
while (userInput != terminationPhrase);

Console.WriteLine("Thank you for using the Financial Advisor!");