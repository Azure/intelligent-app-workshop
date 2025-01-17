# Lesson 3: Semantic Kernel chatbot with plugins

In this lesson we will a semantic kernel plugins to be able to retrieve stock pricing.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed (including updating the StockService `apiKey` value in the `appSettings.json` file using the key from [polygon.io](https://polygon.io/dashboard)).

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

    At the prompt enter:

    ```bash
    What is the current date?
    ```

    Assistant will give a similar response:

    ```txt
    Assistant > I can't access today's date, but imagine it’s an eternal "Fri-yay," ready for financial fun! How can I help you on this hypothetical day?
    ```

1. Notice it does not provide a specific answer. We can use a Semantic Kernel Plugin to be able to fix that.

1. In the `Plugins` directory from `Core.Utilities` directory review the file named `TimeInformationPlugin.cs` which has the following content:

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

1. Next locate **TODO: Step 1** in `Program.cs` and add the following import line:

    ```csharp
    using Core.Utilities.Plugins;
    ```

1. Next locate **TODO: Step 2** in `Program.cs` and provide the following line to register the `TimeInformationPlugin`:

    ```csharp
    kernel.Plugins.AddFromObject(new TimeInformationPlugin());
    ```

1. Next locate **TODO: Step 3** and add the following line to be able to auto invoke kernel functions:

    ```csharp
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
    ```

1. Next locate **TODO: Step 4** and add the following parameters:

    ```csharp
        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, promptExecutionSettings, kernel))
    ```

1. Re-run the program and ask what the current date is. The current date should be displayed this time:

    ```bash
    dotnet run
    What is the current date?
    ```

    Assistant response:

    ```txt
    Assistant > Today's date is October 4, 2024. Time flies like an arrow; fruit flies like a banana! 
    ```

1. Congratulations you are now using your first Semantic Kernel plugin! Next, we are going to leverage another plugin
   that will provide a `StockService`.  This plugin is included within the `Core.Utilities` project.
   Review the file named `StockDataPlugin.cs` from `Core.Utilities\Plugins` which includes 2 functions,
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

1. Next, locate **TODO: Step 5** in `Program.cs` and add import required for `StockService`:

    ```csharp
    using Core.Utilities.Services;
    ```

1. Next locate **TODO: Step 6** and provide the following line to register the new `StockDataPlugin`:

    ```csharp
    HttpClient httpClient = new();
    StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
    kernel.Plugins.AddFromObject(stockDataPlugin);
    ```

1. Next run program and ask stock pricing information:

    ```bash
    dotnet run
    What is MSFT price?

    ```

    Assistant response:

    ```txt
    Assistant > Hold onto your calculators! The price of MSFT is currently $417.63. 
    Looks like it's trying to outshine the stars! 
    ```
