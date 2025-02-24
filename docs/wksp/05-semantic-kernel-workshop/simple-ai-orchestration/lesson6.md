# Lesson 6: Create Grounded Agent with Azure AI Agent service

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
    var bingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;

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
    Assistant > Assistant > **Microsoft (MSFT) Stock Sentiment: 9/10**

    **Recommendation: Buy**

    **Reasoning:**
    - Microsoft's stock has a consensus rating of "Moderate Buy" based on ratings from 29 Wall Street analysts. Out of these, 26 analysts have given a "Buy" rating, while only 3 have given a "Hold" rating【5†source】.
    - The average price target for Microsoft is $510.96, which represents a 25.10% upside from the current price of $408.43【5†source】.
    - The strong consensus among analysts and significant potential upside suggest a solid buy opportunity.

    **Source:**
    MarketBeat, Microsoft (MSFT) Stock Forecast and Price Target 2025【5†source】.
    ```
