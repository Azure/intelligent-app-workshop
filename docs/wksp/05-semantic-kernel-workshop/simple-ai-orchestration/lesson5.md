# Lesson 5: Semantic Kernel chatbot with Chat Completion Agent

In this lesson we will introduce a Chat Completion Agent. The [Chat Completion Agent](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/chat-completion-agent?pivots=programming-language-csharp) is one of the agents available in the Semantic Kernel Agent framework. This agent has specific instructions to provide sentiment analysis on stocks. Note that the [Agent Framework](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp) is currently in preview and subject to change.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.

1. Switch to Lesson 5 directory:

    ```bash
    cd ../Lesson4
    ```

1. Start by copying `appsettings.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Run program and ask what the sentiment on Microsoft stock is:

    ```bash
    dotnet run
    ```

1. At the prompt enter:

    ```bash
    What is the sentiment on Microsoft stock?
    ```

    Assistant will give a generic response:

    ```txt
    Assistant > The sentiment on Microsoft (ticker symbol: MSFT) largely hinges on factors like:

        - Tech innovation (e.g., AI, Azure cloud service, and gaming)
        - Quarterly earnings reports
        - Overall market conditions
        - How much caffeine traders have consumed
    ```

    Notice it does not provide a specific answer. We will add introduce a Stock Sentiment agent to provide a more specific answer.

1. Next locate **TODO: Step 1** in `Program.cs` and add the following import lines:

    ```csharp
    using Microsoft.SemanticKernel.Agents;
    ```

1. Next locate **TODO: Step 2 - Add code to create Stock Sentiment Agent** in `Program.cs` and provide the following lines to initialize the `StockSentimentAgent` using `ChatCompletionAgent`:

    ```csharp
    ChatCompletionAgent stockSentimentAgent =
        new()
        {
            Name = "StockSentimentAgent",
            Instructions =
                """
                Your responsibility is to find the stock sentiment for a given Stock.

                RULES:
                - Use stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
                - Provide the rating in your response and a recommendation to buy, hold or sell.
                - Include the reasoning behind your recommendation.
                - Include the source of the sentiment in your response.
                """,
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { 
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
        };
    ```

1. Locate **TODO: Step 3 - Replace chatCompletionService with stockSentimentAgent** and replace the `foreach` line to use the `stockSentimentAgent` instead of the `chatCompletionService` as follows:

    ```bash
        await foreach (var chatUpdate in stockSentimentAgent.InvokeAsync(chatHistory, kernelArgs))
    ```

1. Re-run the program and ask what the sentiment on Microsoft stock is:

    ```bash
    dotnet run
    User > What is the sentiment on Microsoft stock?
    ```

    Assistant response:

    ```txt
    Assistant > Based on its current performance, Microsoft's stock price (MSFT) is at $408.43, reflecting a strong position as a leading tech giant known for its robust ecosystem and diversified revenue streams.

    Stock Sentiment: **8 out of 10**
    Recommendation: **Buy**

    Reasoning:
    1. Microsoft's cloud computing segment, Azure, is growing rapidly and gaining market share.
    2. The company's involvement in AI and other cutting-edge technologies positions it for long-term growth.
    3. Continuous expansion into lucrative markets like gaming (Xbox) and enterprise software keeps its portfolio resilient.

    However, as with all investments, ensure you're comfortable with the valuation and market conditions before diving in!

    Source: Current stock price from live data. 
    ```

Expect to see a more specific response. Notice it provides a rating on 1 to 10 and recommendation to sell, hold or buy as specified in the agent instructions, however the only live data used for this recommendation is the stock price, so I would not trust this advice blindly. On the next lesson we will introduce grounding with Bing search to be able to retrieve more up to date data grounded using Bing Search data.
