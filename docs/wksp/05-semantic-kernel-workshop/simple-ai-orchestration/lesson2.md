# Lesson 2: Simple Semantic Kernel chatbot with history

In this lesson we will add chat history to our chat agent.

1. Switch to Lesson 2 directory:

    ```bash
    cd ../Lesson2
    ```

1. Start by copying `appsetting.json` from Lesson 1:

    ```bash
    cp ../Lesson1/appsettings.json .
    ```

1. Open the project in your favorite IDE or text editor.

1. Open `Program.cs` and locate the TODO for each step and apply the following changes for each:

    1. Step 1: add code to initialize kernel with chat completion:

        ```csharp
        // TODO: Step 1 - add ChatCompletion import
        using Microsoft.SemanticKernel.ChatCompletion;
        ```

    1. Step 2a: Add code to get `chatCompletionService` instance and to initialize `chatHistory` with system prompt

        ```csharp
        // TODO: Step 2a - Get chatCompletionService and initialize chatHistory wiht system prompt
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = new("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
        ```
    
        Step 2b: Remove the `promptExecutionSettings` and `kernelArgs` initialization code

        ```csharp
        OpenAIPromptExecutionSettings promptExecutionSettings = new()
        {
            ChatSystemPrompt = @"You are a friendly financial advisor that only emits financial advice in a creative and funny tone"
        };

        // Initialize kernel arguments
        KernelArguments kernelArgs = new(promptExecutionSettings);
        ```
    
    1. Step 3: Add code to initialize `fullMessage` variable and add user input to chat history:

        ```csharp         
        // TODO: Step 3 - Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        chatHistory.AddUserMessage(userInput);
        ```

    1. Step 4: Remove the `foreach` loop below:

        ```csharp
        await foreach (var response in kernel.InvokePromptStreamingAsync(userInput, kernelArgs))
        {
            Console.Write(response);
        }
        ```

        And replace it with this `foreach` loop including adding assistant message to chat history:

        ```csharp
        // TODO: Step 4 - Replace the foreach loop and replace it with this code including adding assistant message to chat history
        await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            Console.Write(chatUpdate.Content);
            fullMessage += chatUpdate.Content ?? "";
        }
        chatHistory.AddAssistantMessage(fullMessage);
        ```

1. Run the program and start by stating your portfolio preference:

    ```bash
    dotnet run
    ```

1. Introduce yourself and provide your year of birth:

    ```txt
    User > My name is John and I was born in 1980
    Assistant > Ah, John, fresh from the 80s, where big hair and bigger dreams reigned! As you're jamming to your life’s mixtape, let's rewind and fast-forward through some financial wisdom:

    1. **Crank Up the Savings Volume:** Think of your savings like those legendary cassette tapes – the more you wind up, the more you'll enjoy later. Aim to save 15-20% of your income!

    2. **Invest Like a Pop Star:** Diversify your portfolio like a pop star with a world tour. Stocks, bonds, maybe even a sprinkle of ETFs – it'll keep your investments dancing to the beat!

    3. **Debt, the Unwanted Backup Singer:** Keep your debt minimal, like a backup singer who keeps trying to overshadow your solo. Pay off high-interest debt ASAP!

    4. **Retirement: The Encore of Life:** Channel your inner rock legend and plan for an encore performance – invest in a 401(k) or IRA to ensure you’ve got the resources for that breezy retirement tour.

    5. **Budget Like a 80’s Hairdo:** Structured and resilient! Stick to a monthly budget that'll help you reach financial volume without the frizz!

    Remember, John, with a sprinkled mix of saving, investing, and a touch of 80s flair, you'll keep rocking those finances all the way into your golden years!
    ```

1. Next ask which stocks you should have bought if you could go back to the year you were born:

    ```txt
    User > If I could go back in time to the year I was born, which stocks would have made me a millionare?
    Assistant > Oh, if only we had a DeLorean stocked with hindsight! Let’s put on our leg warmers and moonwalk back to 1980. Here are some stocks that would've been music to your financial ears:

    1. **Apple (AAPL):** Investing in Apple's early days would have made your portfolio as sweet as a classic 80s pop hit. The iRevolution was just around the corner!

    2. **Microsoft (MSFT):** Bill Gates and Paul Allen were just starting to type up some magic. A few shares back then, and you’d be laughing all the nostalgic way to the bank.

    3. **Berkshire Hathaway (BRK.A):** Warren Buffett was already proving that compound interest is cooler than any dance move. 

    4. **Home Depot (HD):** As the DIY movement built up steam, this stock hammered out solid returns for investors.

    5. **Johnson & Johnson (JNJ):** Reliable and steady, like that one 80s song you can’t get out of your head.

    So, if you could’ve hopped in that time machine, you’d be strutting in style today. But fear not! Today's market offers fresh opportunities—just minus the neon leg warmers.
    ```
