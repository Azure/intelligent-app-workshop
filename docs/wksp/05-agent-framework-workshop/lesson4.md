# Lesson 4: Web Search Integration for Enhanced Analysis

This lesson adds web search capabilities to our financial agent, allowing it to access current market news and analysis for more comprehensive responses.

1. Switch to Lesson 4 directory:

    ```bash
    cd workshop/dotnet/Lessons/Lesson4
    ```

    ```powershell
    cd workshop/dotnet/Lessons/Lesson4
    ```

1. Copy the configuration file from the Solutions directory:

    ```powershell
    mv ../Lesson1/appsettings.json appSettings.json
    ```

    ```bash
    cp ../Lesson1/appsettings.json appSettings.json
    ```

1. Open `Program.cs` and add web search capabilities to the financial agent:

    1. **TODO: Step 1** - Add Azure Foundry Environment variables

        ```csharp
        // Set Azure AI and Authentication environment variables (required for Azure AI Foundry agent)
        Environment.SetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_ENDPOINT", applicationSettings.AIFoundryProject.Endpoint);
        Environment.SetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_DEPLOYMENT_NAME", applicationSettings.AIFoundryProject.DeploymentName);

        // Set Bing Grounding Connection ID (Required for web search functionality)  
        Environment.SetEnvironmentVariable("BING_CONNECTION_ID", applicationSettings.AIFoundryProject.GroundingWithBingConnectionId);
        ```

    1. **TODO** Step2 - Create a persistent client
    ```csharp
        // Create PersistentAgentsClient for Azure AI Foundry
        var persistentAgentsClient = new PersistentAgentsClient(
            applicationSettings.AIFoundryProject.ConnectionString,
            new DefaultAzureCredential());
    ```

    1. **TODO** Step 3 - Create web search tool for Bing grounding (requires BING_CONNECTION_ID environment variable)

        ```csharp
        HostedWebSearchTool webSearchTool = new();
        ```

    1. ** TODO ** Step 4 - Stock Sentiment Agent system instructions and initialization - defines the agent's behavior and rules
        ```csharp
        string stockSentimentAgentInstructions = """
            You are a Financial Analysis Agent with web search capabilities. Provide direct, comprehensive financial analysis and insights based on user questions.

            CAPABILITIES:
            - Analyze individual stocks, market sectors, or broader financial topics
            - Extract stock symbols from user queries when relevant (e.g., "What do you think about Microsoft?" -> analyze MSFT)
            - Handle free-form questions about market trends, economic conditions, investment strategies
            - Use stock sentiment scale from 1 to 10 where sentiment is 1 for sell and 10 for buy (when analyzing specific stocks)
            - Provide ratings, recommendations (buy/hold/sell), and detailed reasoning for stock-specific queries

            CRITICAL RULES:
            - Provide your complete analysis in a SINGLE response - do not say you're "gathering data" or "working on it"
            - For stock-specific questions: Use web search to gather current market news, analyst opinions, and sentiment data
            - For general financial questions: Use web search to find relevant financial news, economic data, and expert analysis
            - Combine web search results with available stock price data when analyzing specific companies
            - ALWAYS include a dedicated "Sources" section at the end of your response listing all the specific sources you found through web search
            - For each source, include the title, URL (if available), and a brief description of what information it provided
            - Focus on recent news, market trends, and expert analysis
            - Be transparent about which information came from which sources
            - If a user asks about a specific company without mentioning the stock symbol, try to identify the relevant ticker symbol
            - Answer immediately with your full analysis - do not provide status updates or say you're collecting information
            """;

        // Create Financial Analysis Agent in Azure AI Foundry (following GitHub example pattern)
        // Create agent with Bing grounding tool, then pass local function tools at runtime via ChatClientAgentRunOptions
        var agent = await persistentAgentsClient.CreateAIAgentAsync(
            applicationSettings.AIFoundryProject.DeploymentName,
            instructions: stockSentimentAgentInstructions,
            tools: [ 
                new BingGroundingToolDefinition(
                    new BingGroundingSearchToolParameters(
                        new[] { 
                            new BingGroundingSearchConfiguration(
                                applicationSettings.AIFoundryProject.GroundingWithBingConnectionId
                            ) 
                        }
                    )
                ) 
            ]
        );

        // Create a thread for conversation
        var thread = agent.GetNewThread();

        // Create run options with local function tools (following GitHub example)
        var agentOptions = new ChatClientAgentRunOptions(new() { 
            Tools = [
                timeTool,
                stockPriceTool,
                stockPriceDateTool,
                webSearchTool  // This will use the BING_CONNECTION_ID for foundry grounding
            ] 
        });

        ```

1. Test the enhanced agent with various financial queries:

    ```bash
    dotnet run
    ```

    Example questions to test:
    ```
    User > What do you think about Microsoft?
    Assistant > (should search for recent Microsoft news and provide analysis with sources)
    
    User > How is the tech sector performing?
    Assistant > (should search for tech sector news and provide comprehensive analysis)
    
    User > Should I invest in renewable energy stocks?
    Assistant > (should search for renewable energy market trends and provide recommendations)
    ```

This lesson demonstrates how web search integration enhances the agent's ability to provide current, well-sourced financial analysis by accessing real-time market information and expert opinions.