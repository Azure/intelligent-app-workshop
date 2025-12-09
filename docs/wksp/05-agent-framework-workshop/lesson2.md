# Lesson 2: Adding Function Calling with Agent Framework

This lesson builds on lesson 1 by adding function calling capabilities to our agent. Function calling allows the AI model to invoke external functions to get real-time data or perform computations.

1. Switch to Lesson 2 directory:

    ```powershell
    cd workshop/dotnet/Lessons/Lesson2
    ```

1. Copy the configuration file from the Solutions directory:

    ```powershell
    cp ../Lesson1/appsettings.json appSettings.json
    ```

1. Observe the plugins in the `Core.Utilities.Plugins` namespace:

    - `TimeInformationPlugin.cs` - Provides current UTC time
    - `StockDataPlugin.cs` - Provides stock price data

1. Open `Program.cs` and add the following features:

    1. **TODO: Step 1** - Initialize the chat client and plugins:

        ```csharp
        using Core.Utilities.Plugins;
        using Core.Utilities.Services;

        IChatClient chatClient = AgentFrameworkProvider.CreateChatClientWithApiKey();

        // Initialize plugins
        TimeInformationPlugin timePlugin = new();
        HttpClient httpClient = new();
        StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
        ```

    1. **TODO: Step 2** - Create AI functions using AIFunctionFactory:

        ```csharp
        var tools = new AIFunction[]
        {
            AIFunctionFactory.Create(timePlugin.GetCurrentUtcTime),
            AIFunctionFactory.Create(stockDataPlugin.GetStockPrice),
            AIFunctionFactory.Create(stockDataPlugin.GetStockPriceForDate)
        };
        ```

    1. **TODO: Step 3** - Create a ChatClientAgent with function calling capabilities:

        ```csharp
        string systemInstructions = "You are a friendly financial advisor that only emits financial advice in a creative and funny tone";

        ChatClientAgent agent = new(
            chatClient,
            instructions: systemInstructions,
            name: "FinancialAdvisor",
            description: "A friendly financial advisor with access to time and stock data",
            tools: tools
        );
        ```

    1. **TODO: Step 4** - Create thread and use agent:

        ```csharp
        AgentThread thread = agent.GetNewThread();
        var response = await agent.RunAsync(userInput, thread);
        Console.WriteLine(response);
        ```

1. Test the application by asking questions that require function calls:

    ```bash
    dotnet run
    ```

    Example questions to test:
    ```
    User > What time is it?
    Assistant > (should call time function and provide current time)
    
    User > What is the current price of MSFT?
    Assistant > (should call stock price function and provide current Microsoft stock price)
    
    User > What was the price of AAPL on 2023-01-01?
    Assistant > (should call historical stock price function)
    ```

The Agent Framework automatically handles function calling - when the model determines it needs external data, it will invoke the appropriate function and use the result in its response.