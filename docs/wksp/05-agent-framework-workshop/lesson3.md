# Lesson 3: Specialized Agent with Stock Sentiment Analysis

This lesson creates a specialized agent focused on stock sentiment analysis using the Agent Framework with specific system instructions and function calling capabilities.

1. Copy the configuration file from the Solutions directory:

    ```powershell
    mv ../Lesson1/appsettings.json appSettings.json
    ```

    ```bash
    cp ../Lesson1/appsettings.json appSettings.json
    ```

1. Open `Program.cs` and build a specialized stock sentiment agent:

    1. **TODO: Step 1** - Define specialized system instructions for stock sentiment analysis:

        ```csharp
        string stockSentimentAgentInstructions = """
            You are a Stock Sentiment Agent. Your responsibility is to find the stock sentiment for a given Stock.

            RULES:
            - Use stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
            - Provide the rating in your response and a recommendation to buy, hold or sell.
            - Include the reasoning behind your recommendation.
            - Include the source of the sentiment in your response.
            - Focus on technical analysis based on stock price data and general market knowledge.
            """;
        ```

    1. **TODO: Step 2** - Create the specialized Stock Sentiment Agent:

        ```csharp
        ChatClientAgent agent = new(
            chatClient,
            instructions: stockSentimentAgentInstructions,
            name: "StockSentimentAgent",
            description: "An intelligent agent that analyzes stock sentiment using market data",
            tools: tools
        );
        ```

1. Test the specialized agent with stock sentiment queries:

    ```bash
    dotnet run
    ```

    Example questions to test:
    ```
    User > What is your sentiment on MSFT?
    Assistant > (should analyze Microsoft stock and provide sentiment rating 1-10 with buy/hold/sell recommendation)
    
    User > Analyze AAPL sentiment
    Assistant > (should provide Apple stock sentiment analysis with reasoning)
    
    User > Should I buy TSLA?
    Assistant > (should analyze Tesla and provide specific buy/hold/sell recommendation)
    ```

This lesson demonstrates how to create specialized agents with focused system instructions and domain-specific capabilities using the Agent Framework.