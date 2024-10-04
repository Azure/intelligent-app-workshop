# Lesson 2: Simple Semantic Kernel chat agent with plugins

In this lesson we will a semantic kernel plugins to be able to retrieve stock pricing.

1. Switch to Lesson 2 directory:

    ```bash
    cd Lessons/Lesson2
    ```

1. Start by copying `appsetting.json` from Lesson 1:

    ```bash
    cp ../Lessons/Lesson1/appsettings.json .
    ```

1. Run program and ask what the current date is:

    ```bash
    dotnet run
    User > what is the current date?
    Assistant > I can't access today's date, but imagine it’s an eternal "Fri-yay," ready for financial fun! How can I help you on this hypothetical day?
    ```

1. Notice it does not provide a specific answer. Let's create a Semantic Kernel Plugin to be able to fix that. 
   On the `Lesson2` directory create a new file name `TimeInformationPlugin.cs` in the same directory as `Program.cs` 
   and add the following content

    ```csharp
    using System.ComponentModel;
    using Microsoft.SemanticKernel;

    public class TimeInformationPlugin
    {
        [KernelFunction] 
        [Description("Retrieves the current time in UTC.")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
    ```

1. Next locate Step 1 in `Program.cs` and provide the following line to register the `TimeInformationPlugin`:

    ```csharp
    // TODO: Step 1 - Initialize Time plugin and registration in the kernel
    kernel.Plugins.AddFromObject(new TimeInformationPlugin());
    ```

1. Next locate Step 2 and add the following line (including the comma at the end) to be able to 
   auto invoke kernel functions:

    ```csharp
        // Step 2 - Add Auto invoke kernel functions as the tool call behavior
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
    ```

1. Re-run the program and ask what the current date is. The current date should be displayed this time:

    ```bash
    dotnet run
    User > what is the current date?
    Assistant > Today's date is October 4, 2024. Time flies like an arrow; fruit flies like a banana! 
    ```

1. Congratulations you have written your first Semantic Kernel plugin! Next, we are going to add another plugin
   that will integrate to be able to an existing `StockService` included within the `Core.Utilities` project.
   Create a new file named `StockDataPlugin.cs` in the same directory as `Program.cs` and include the following code,
   which includes 2 functions, one to retrieve the stock price for the current date and another one for a specific date:

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

1. Next, locate Step 3 in `Program.cs` and add import required for `StockService`:

    ```csharp
    // TODO: Step 3 - Add import required for StockService
    using Core.Utilities.Services;
    ```

1. Next locate Step 4  and provide the following line to register the new `StockDataPlugin`:

    ```csharp
    HttpClient httpClient = new();
    StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
    kernel.Plugins.AddFromObject(stockDataPlugin);
    ```

1. Before we can run this, you need to get an API Key to be able to get stock prices from: 
   https://polygon.io/dashboard/login. You can sign up for a free API Key by creating a login.

1. Once the apiKey is provide, update the `appSettings.json` and paste it into this line:

    ```json
      "stockServiceApiKey": "<key>"
    ```

1. Next run program and ask stock pricing information:

    ```bash
    User > what is MSFT price?
    Assistant > Hold onto your calculators! The price of MSFT is currently $417.63. 
    Looks like it's trying to outshine the stars! 
    ```