# Lesson 6: Create Semantic Kernel chat completion agent

In this lesson, we will add a Semantic Kernel Azure AI agent to our chatbot program. This agent will be a Stock Sentiment agent to provide a recommendation to buy, hold, or sell a stock based on a stock sentiment rating.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.

1. Switch to Lesson 6 directory:

    ```bash
    cd ../Lesson6
    ```

1. Start by copying `appsettings.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Run the program to validate the code is functional:

    ```bash
    dotnet run
    ```

1. Next, locate **TODO: Step 1 -- Add imports for Agents and Azure.Identity** in `Program.cs` and add the following imports:

    ```csharp
    using Azure.AI.Projects;
    using Azure.Identity;
    using Microsoft.SemanticKernel.Agents.AzureAI;
    ```

1. Next, locate **TODO: Step 2 - Initialize connection to Grounding with Bing Search tool and agent** in `Program.cs` and add the following code. Be sure to copy the connectionString and bingConnectionId from Azure AI Foundry:

    ```csharp
    var connectionString = AISettingsProvider.GetSettings().AIFoundryProject.ConnectionString;
    var groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;

    var projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());
                
    ConnectionResponse bingConnection = await projectClient.GetConnectionsClient().GetConnectionAsync(bingConnectionId);
    var connectionId = bingConnection.Id;

    ToolConnectionList connectionList = new ToolConnectionList
    {
        ConnectionList = { new ToolConnection(connectionId) }
    };
    BingGroundingToolDefinition bingGroundingTool = new BingGroundingToolDefinition(connectionList);

    var clientProvider =  AzureAIClientProvider.FromConnectionString(connectionString, new AzureCliCredential());
    AgentsClient client = clientProvider.Client.GetAgentsClient();
    var definition = await client.CreateAgentAsync(
        "gpt-4o",
        instructions:
                """
                Your responsibility is to find the stock sentiment for a given Stock.

                RULES:
                - Report a stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
                - Only use current data reputable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
                - Provide the stock sentiment scale in your response and a recommendation to buy, hold or sell.
                - Include the reasoning behind your recommendation.
                - Be sure to cite the source of the information.
                """,
        tools:
        [
            bingGroundingTool
        ]);
    var agent = new AzureAIAgent(definition, clientProvider)
    {
        Kernel = kernel,
    };

    // Create a thread for the agent conversation.
    AgentThread thread = await client.CreateThreadAsync();
    ```

1. Next, locate **TODO: Step 3 - Uncomment out all code after "Execute program" comment** in `Program.cs` and uncomment the previously commented block of code:

1. Finally, locate **TODO: Step 4 - Invoke the agent** in `Program.cs` and implement this code:
```csharp
        ChatMessageContent message = new(AuthorRole.User, userInput);
        await agent.AddChatMessageAsync(thread.Id, message);

        await foreach (ChatMessageContent response in agent.InvokeAsync(thread.Id))
        {
            string contentExpression = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : response.Content;
            Console.WriteLine($"{contentExpression}");
        }
```

1. Re-run the program and ask for the stock sentiment on Microsoft, you should see an output similar to this:

    ```txt
    User > what is the stock sentiment on Microsoft?
    Assistant > As of February 2025, the sentiment for Microsoft (MSFT) stock is generally positive. The stock is rated as a "Moderate Buy" by 29 Wall Street analysts, with 26 advising a "Buy" and 3 recommending a "Hold"【5†source】. The average price target for the stock is $510.96, which suggests a potential upside of about 23.95% from its current price around $412【6†source】.

    **Recommendation: Buy**

    **Reasoning:**
    - **Analyst Consensus:** The overwhelming majority of analysts recommend buying MSFT, indicating confidence in its future performance.
    - **Price Target Upside:** With a considerable potential price increase expected, it seems like a promising time to invest.
    - **Market Confidence:** The stock reflects strong investor confidence and potential growth【5†source】【6†source】.

    These indicators support the recommendation to buy Microsoft stock at this time.
    ```
