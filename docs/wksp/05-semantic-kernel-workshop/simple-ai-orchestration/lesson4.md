# Lesson 4: Describe all plugins in Semantic Kernel chatbot

In this lesson we will add functionality to list all plugins and plugin parameters that are loaded in the application's Semantic Kernel instance.

1. Ensure all [pre-requisites](pre-reqs.md) are met and installed.

1. Switch to Lesson 5 directory:

    ```bash
    cd ../Lesson4
    ```

1. Start by copying `appsettings.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Run program to validate the code is functional:

    ```bash
    dotnet run
    ```

1. Locate **TODO: Step 1 - Add import for ModelExtensionMethods** in `Program.cs` and add the following import:

    ```csharp
    using Core.Utilities.Extensions;
    ```

1. Next locate **TODO: Step 2 - add call to print all plugins and functions** in `Program.cs` and add the following lines to print out kernel plugins info:

    ```csharp
    var functions = kernel.Plugins.GetFunctionsMetadata();
    Console.WriteLine(functions.ToPrintableString());
    ```

1. Next locate **TODO: Step 3 - Comment out all code after "Execute program" comment** and comment all lines of code after the `//Execute program` line.

    ```csharp
    // TODO: Step 3 - Comment out all code after "Execute program" comment
    // Execute program.
    /*
    const string terminationPhrase = "quit";
    ...
    while (userInput != terminationPhrase);
    */
    ```

1. Re-run the program and you should see an output similar to this:

    ```bash
    **********************************************
    ****** Registered plugins and functions ******
    **********************************************

    Plugin: GetCurrentUtcTime
    GetCurrentUtcTime: Retrieves the current time in UTC.

    Plugin: GetStockPrice
    GetStockPrice: Gets stock price
        Params:
        - symbol:
            default: ''

    Plugin: GetStockPriceForDate
    GetStockPriceForDate: Gets stock price for a given date
        Params:
        - symbol:
            default: ''
        - date:
            default: ''
    ```

1. Review the `Core.Utilities.Extensions.ModelExtensionMethods` class in the `CoreUtilities` project to understand how the plugins are traversed to print out plugins and corresponding plugin parameters information.
