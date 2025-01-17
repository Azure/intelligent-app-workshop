# Lesson 4: Semantic Kernel chatbot with Web Search engine plugin

In this lesson we will add a Web Search Engine plugin that uses Bing Search to our semantic kernel chatbot.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed (including updating the `BingSearchService` `apiKey` value in the `appSettings.json` file using the key from **Bing Search Service v7** in [Azure Portal](https://portal.azure.com).

1. Switch to Lesson 4 directory:

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

    Notice it does not provide a specific answer. We can add the Web Search Engine plugin to be able to provide a better answer.

1. Next locate **TODO: Step 1** in `Program.cs` and add the following import lines:

    ```csharp
    using Microsoft.SemanticKernel.Plugins.Web;
    using Microsoft.SemanticKernel.Plugins.Web.Bing;
    ```

1. Next locate **TODO: Step 2** in `Program.cs` and provide the following lines to initialize and register the `WebSearchEnginePlugin`:

    ```csharp
    var bingApiKey = AISettingsProvider.GetSettings().BingSearchService.ApiKey;
    if (!string.IsNullOrEmpty(bingApiKey))
    {
        var bingConnector = new BingConnector(bingApiKey);
        var bing = new WebSearchEnginePlugin(bingConnector);
        kernel.ImportPluginFromObject(bing, "bing");
    }
    ```

1. Re-run the program and ask what the sentiment on Microsoft stock is:

    ```bash
    dotnet run
    User > What is the sentiment on Microsoft stock?
    ```

    Assistant response:

    ```txt
    Assistant > Ah, Microsoft stock seems to be the belle of the ball with mixed but leaning-positive vibes. Let me serve up the sentiment soup:

    - **Positives:** There's more excitement than usual, with higher-than-average media sentiment compared to other tech companies. Plus, analysts are dishing out 12-month price targets like cocktails, with an average around $489.55 and some saying it could go as high as $600—a nice little pie in the sky for investors! 🍰🚀

    - **Concerns:** Clouds (pun intended!) aren't all silver-lining for Microsoft, as they've got capacity constraints in their cloud services. Choppy waters ahead, perhaps? 🌩️

    - **Buzz:** Oh, it's trending alright! Tons of people are searching, sharing, and probably debating MSFT more than their weekend plans.

    Feeling FOMO or ready to YOLO-invest? Don’t forget to watch that ticker—MSFT!    
    ```

Expect to see a more specific response. With the Web Search Engine plugin, you can now tap into any web search, so your agent will leverage that plugin to find information not available via other plugins or within the LLM being used.
