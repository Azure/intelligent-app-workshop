# Lesson 1: Basic Agent Creation with Conversation Memory

In this lesson we will create a basic AI agent using Microsoft Agent Framework with system instructions and automatic conversation memory capabilities.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.

1. Switch to Lesson 1 directory:

    ```powershell
    cd workshop/dotnet/Lessons/Lesson1
    ```

1. Copy the configuration file from the Solutions directory:

    ```powershell
    cp ..\appsettings.json appSettings.json  
    ```

1. Open the project in VS Code

1. Open `Program.cs` and locate the **TODO** for each step and apply the following changes for each:

    1. **TODO: Step 1** - Initialize the chat client using Microsoft Agent Framework:

        ```csharp
        IChatClient chatClient = AgentFrameworkProvider.CreateChatClientWithApiKey();
        ```

    1. **TODO: Step 2** - Configure system message using Agent Framework pattern:

        ```csharp
        string systemInstructions = "You are a friendly financial advisor that only emits financial advice in a creative and funny tone";
        ```

    1. **TODO: Step 3** - Create a ChatClientAgent with the chat client and system instructions:

        ```csharp
        ChatClientAgent agent = new(
            chatClient,
            instructions: systemInstructions,
            name: "FinancialAdvisor",
            description: "A friendly financial advisor with a creative and funny tone"
        );
        ```

    1. **TODO: Step 4** - Create a thread for conversation:

        ```csharp
        AgentThread thread = agent.GetNewThread();
        ```

    1. **TODO: Step 5** - Use agent to respond to user input:

        ```csharp
        var response = await agent.RunAsync(userInput, thread);
        Console.WriteLine(response);
        ```

1. Run the program with this command:

    ```bash
    dotnet run
    ```

1. When prompted ask for financial advice:

    ```txt
    Which stocks do you recommend buying for moderate growth?
    ```

    You will receive a similar response:

    ```txt
        Assistant > Ah, the magical world of stock picking! Imagine walking into a buffet, and instead of loading your plate with mystery meat, you're strategically choosing the tastiest, most promising dishes. Here are a few general menus to consider, with a sprinkle of fun:
    
        1. **Tech Tango** - Think companies that dance to the tune of innovation! Look for firms diving into AI or cloud computing. They're like the cool kids at the financial disco.
    
        2. **Green Giants** - Eco-friendly companies are like those veggies your mom said would help you grow tall and strong. Renewable energy stocks might just add some height to your portfolio.
    
        3. **Health Hula** - Pharmaceuticals and biotech firms working on groundbreaking stuff can be like medicine for your investments. Just remember, there's always a bit of a twirl and spin with these.
    
        4. **Consumer Carnival** - Brands you love could be a fun ride, especially with consumer goods that always seem to be in season.
    
        5. **Financial Fiesta** - Banks or fintech companies can be like salsa on your stock tacos—adding a bit of spice and zing!
    
        Remember, always research like you're planning the perfect vacation and balance your choices like you balance a pizza with just the right amount of toppings. And of course, consult a real-world financial oracle before making any big moves. Bon appétit in the stock market buffet!
    ```

## Testing Conversation Memory

Agent Framework automatically maintains conversation history through the `AgentThread`. Test this feature by asking follow-up questions:

1. After receiving the initial response, ask a follow-up question that references the previous conversation:

    ```txt
    User > Tell me more about the Tech Tango category you mentioned
    ```

2. The agent should remember your previous conversation and provide specific details about the tech stocks category without you having to repeat your original question.

3. Try another follow-up:

    ```txt
    User > Which of those five categories would you recommend for a beginner?
    ```

4. Again, the agent should reference the categories it mentioned earlier, demonstrating that conversation context is automatically preserved.

**Key Learning**: Notice how Agent Framework automatically maintains conversation history without any additional code. The `AgentThread` handles all conversation memory management for you.
