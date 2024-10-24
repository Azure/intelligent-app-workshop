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
        // TODO: Step 1 - Initialize Time plugin and registration in the kernel
        kernel.Plugins.AddFromObject(new TimeInformationPlugin());
        // Step 1 - add ChatCompletion import
        using Microsoft.SemanticKernel.ChatCompletion;
        ```

    1. Step 2a: Get `chatCompletionService` and initialize `chatHistory` wiht system prompt

        ```csharp
        // TODO: Step 2a - Get chatCompletionService and initialize chatHistory wiht system prompt
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory chatHistory = new("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
        ```
    
        Step 2b: Remove the `promptExecutionSettings` and `kernelArgs` initialization code

        ```csharp
        // Add system prompt
        OpenAIPromptExecutionSettings promptExecutionSettings = new()
        {
            ChatSystemPrompt = @"You are a friendly financial advisor that only emits financial advice in a creative and funny tone"
        };

        // Initialize kernel arguments
        KernelArguments kernelArgs = new(promptExecutionSettings);
        ```
    
    1. Step 3: Initialize `fullMessage` variable and add user input to chat history:

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
    ```bash 
    User > My portfolio preference is moderate growth
    ```
    Assistant > Ah, the sweet spot of moderation! You're like the Goldilocks of investing, not too hot, not too cold, but just right. Let's sprinkle some fairy dust on your portfolio and make it grow!
   

1. Next ask what your portfolio preferenc is:

    ```bash
    User > what is my portfolio preference?
    ```
    Assistant > Ah, you're the maestro of moderate growth! You're seeking a harmonious balance between risk and return, like a blend of jazz and rock in the symphony of investing. A sprinkle of thrill, a dash of stability, all wrapped in a comfy financial blanket.
    
