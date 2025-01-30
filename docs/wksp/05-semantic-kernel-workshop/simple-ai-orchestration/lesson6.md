# Lesson 6: Create Semantic Kernel chat completion agent

In this lesson we will add a Semantic Kernel chat completion agent to our chatbot program. This agent will be a Stock Sentiment agent to provide a recommendation to buy, hold or sell a stock based on a stock sentiment rating.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.

1. Switch to Lesson 6 directory:

    ```bash
    cd ../Lesson6
    ```

1. Start by copying `appsettings.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Run program to validate the code is functional:

    ```bash
    dotnet run
    ```

1. Next locate **TODO: Step 1 - Add import for Agents** library in `Program.cs`:

    ```csharp
    using Microsoft.SemanticKernel.Agents;
    ```

1. Next locate **TODO: Step 2 - Remove initial prompt** from `chatHistory` initialization in `Program.cs`. This will help ensure the prompt does not interfere with the new agent instructions:

    ```csharp
    ChatHistory chatHistory = new();
    ```

1. Next locate **TODO: Step 3 - Comment out line to print plugins**. This will revert to the prior versions of the program which start with the user prompt.

    ```csharp
    //Console.WriteLine(functions.ToPrintableString());
    ```

1. Locate the **TODO: Step 4 - Add code to create Stock Sentiment Agent** and add the doe below. This introduces our first SemanticKernel agent which has precise instructions to provide buy/hold/sell recommendations based on stock sentiment analysis.

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
                - Only use reliable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
                - Provide the rating in your response and a recommendation to buy, hold or sell.
                - Include the reasoning behind your recommendation.
                - Include the source of the sentiment in your response.
                """,
            Kernel = kernel,
            Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() { 
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()})
        };
    ```

1. Locate **TODO: Step 5 - Uncomment previously commented code** and uncomment the previously commented block of code.

    ```csharp
    // Execute program.
    // TODO: Step 5 - Uncomment previously commented code
    const string terminationPhrase = "quit";
    string? userInput;
    do
    {
        ...
    }
    while (userInput != terminationPhrase);
    ```

1. Finally locate **TODO: Step 6 - Replace chatCompletionService with stockSentimentAgent** and replace the existing `chatCompletionService` line with this line of code:

    ```csharp
        await foreach (var chatUpdate in stockSentimentAgent.InvokeAsync(chatHistory, kernelArgs))
    ```

1. Re-run the program and ask for the stock sentiment on Microsoft, you should see an output similar to this:

    ```txt
    User > what is the stock sentiment on Microsoft?
    Assistant > Based on the available information, here is the stock sentiment for Microsoft (MSFT):

    ### Sentiment Insights:
    1. **Analyst Consensus**:
    - The majority of analysts have a "Buy" or "Strong Buy" rating for Microsoft stock.
    - The average brokerage recommendation (ABR) score for Microsoft is 1.23 on a scale where 1 is "Strong Buy" and 5 is "Strong Sell" (source: Yahoo Finance).
    - Of 19 analysts tracked by Visible Alpha, 18 rate it as a "Buy" or equivalent, with only 1 rating it as "Hold".
    - The consensus price target is approximately $517, which reflects significant upside potential (~16% above the current price).

    2. **Market Sentiment**:
    - Recent sentiment appears bullish among analysts, particularly ahead of earnings and driven by strong performance in enterprise and cloud businesses.

    3. **Stock Forecast and Trends**:
    - Microsoft's long-term prospects remain strong due to its leadership position in cloud computing, productivity software, and AI integrations.

    ---

    ### Sentiment Rating: **9/10**
    - **Recommendation**: **Buy**
    - **Reasoning**: Microsoft's dominant market position, bullish analyst ratings, and high price target support a favorable outlook. The stock appeals for long-term investment based on growth potential in cloud and AI, despite potential short-term market fluctuations.

    ### Sources:
    - [Yahoo Finance - Analyst Recommendations](https://finance.yahoo.com/research/stock-forecast/MSFT/)
    - [Visible Alpha](https://www.nasdaq.com/market-activity/stocks/msft/analyst-research)
    ```
