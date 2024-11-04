# Lesson 3: Simple Semantic Kernel chat agent with plugins

In this lesson we will a semantic kernel plugins to be able to retrieve stock pricing.

1. Switch to Lesson 3 directory:

    ```bash
    cd ../Lesson3
    ```

1. Start by copying `appsettings.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Run program and ask what the current date is:

    ```bash
    dotnet run
    ```
   At the "User >" prompt enter
   ```bash
   What is the current date?
    ```
   Assistant will give a similar response:
   ```
   Assistant > I can't access today's date, but imagine it’s an eternal "Fri-yay," ready for financial fun! How can I help you on this hypothetical day?
   ```
1. Notice it does not provide a specific answer. We can use a Semantic Kernel Plugin to be able to fix that.

 1. In the `Plugins` directory from `Console.Utilities` directory review the file named 
    `TimeInformationPlugin.cs` which has the following content:

    ```csharp
    using System.ComponentModel;
    using Microsoft.SemanticKernel;

    namespace Plugins;

    public class TimeInformationPlugin
    {
        [KernelFunction] 
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
    ```

1. Next locate Step 1 in `Program.cs` and add the following import line:

    ```csharp
    // TODO: Step 1 - Add import for Plugins
    using Console.Utiltiies.Plugins;
    ```

1. Next locate Step 2 in `Program.cs` and provide the following line to register the `TimeInformationPlugin`:

    ```csharp
    // TODO: Step 2 - Initialize Time plugin and registration in the kernel
    kernel.Plugins.AddFromObject(new TimeInformationPlugin());
    ```

1. Next locate Step 3 and add the following line to be able to 
   auto invoke kernel functions:

    ```csharp
        // TODO: Step 3 - Add Auto invoke kernel functions as the tool call behavior
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    ```

1. Next locate Step 4 and add the following parameters:

    ```csharp
        // TODO: Step 4 - Provide promptExecutionSettings and kernel arguments
        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel))
    ```

1. Re-run the program and ask what the current date is. The current date should be displayed this time:

    ```bash
    dotnet run
    User > what is the current date?
    Assistant > Today's date is October 4, 2024. Time flies like an arrow; fruit flies like a banana! 
    ```

1. Congratulations you are now using your first Semantic Kernel plugin! Next, we are going to need another plugin
   that will integrate to be able to an existing `StockService` included within the `Core.Utilities` project.
   Review the file named `StockDataPlugin.cs` from `Console.Utilities\Plugins` which includes 2 functions,
   one to retrieve the stock price for the current date and another one for a specific date:

    ```csharp
    using Core.Utilities.Services;
    using Core.Utilities.Models;
    using Core.Utilities.Extensions;

    using Microsoft.SemanticKernel;
    using System.ComponentModel;

    public class StockDataPlugin(StocksService stockService)
    {
        private readonly StocksService _stockService = stockService;

        [KernelFunction, Description("Gets stock price")]
        public async Task<string> GetStockPrice(string symbol)
        {
            string tabularData = (await _stockService.GetStockDailyOpenClose(symbol)).FormatStockData();
            return tabularData;
        }

        [KernelFunction, Description("Gets stock price for a given date")]
        public async Task<string> GetStockPriceForDate(string symbol, DateTime date)
        {
            string tabularData = (await _stockService.GetStockDailyOpenClose(symbol, date)).FormatStockData();
            return tabularData;
        }

    }
    ```

1. Next, locate Step 5 in `Program.cs` and add import required for `StockService`:

    ```csharp
    // TODO: Step 5 - Add import required for StockService
    using Core.Utilities.Services;
    ```

1. Next locate Step 6 and provide the following line to register the new `StockDataPlugin`:

    ```csharp
    // TODO: Step 6 - Initialize Stock Data Plugin and register it in the kernel
    HttpClient httpClient = new();
    StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
    kernel.Plugins.AddFromObject(stockDataPlugin);
    ```

1. Before we can run this, you need to get an API Key to be able to get stock prices from: 
   [https://polygon.io/dashboard/login](). You can sign up for a free API Key by creating a login.

1. Once the apiKey is provide, update the `appSettings.json` and paste it into this line:

    ```json
      "StockService": {
        "ApiKey": "<key>"
      }
    ```

1. Next run program and ask stock pricing information:

    ```bash
    User > what is MSFT price?
    Assistant > Hold onto your calculators! The price of MSFT is currently $417.63. 
    Looks like it's trying to outshine the stars! 
    ```
