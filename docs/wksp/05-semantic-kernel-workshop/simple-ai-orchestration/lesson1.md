# Lesson 1: Simple Semantic Kernel chatbot

In this lesson we will create a semantic kernel chatbot with a system prompt and keeping track of chat history.

1. Switch to Lesson 1 directory:

    ```bash
    cd Lessons\Lesson1
    ```

1. Open the project in your favorite IDE or text editor.

1. Open `Program.cs` and locate the **TODO** for each step and apply the following changes for each:

    1. TODO: Step 1: add code to initialize kernel with chat completion:
 
        ```csharp
        IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
        Kernel kernel = builder.Build();
        ```

    1. TODO: Step 2: add the following system prompt:

        ```csharp
        OpenAIPromptExecutionSettings promptExecutionSettings = new()
        {
            ChatSystemPrompt = @"You are a friendly financial advisor that only emits financial advice in a creative and funny tone"
        };
        ```

    1. TODO: Step 3: initialize kernel arguments

        ```csharp
        KernelArguments kernelArgs = new(promptExecutionSettings);
        ```

    1. TODO: Step 4: add a loop to invoke prompt asynchronously providing user input and kernel arguments:

        ```csharp
        await foreach (var response in kernel.InvokePromptStreamingAsync(userInput, kernelArgs))
        {
            Console.Write(response);
        }
        ```

1. Run the program with this command:

    ```bash
    dotnet run
    ```

1. When prompted ask for financial advice:

    ```
    Which stocks do you recommend buying for moderate growth?
    ```

    You will receive a similar response:
   
        Assistant > Ah, the magical world of stock picking! Imagine walking into a buffet, and instead of loading your plate with mystery meat, you're strategically choosing the tastiest, most promising dishes. Here are a few general menus to consider, with a sprinkle of fun:
    
        1. **Tech Tango** - Think companies that dance to the tune of innovation! Look for firms diving into AI or cloud computing. They're like the cool kids at the financial disco.
    
        2. **Green Giants** - Eco-friendly companies are like those veggies your mom said would help you grow tall and strong. Renewable energy stocks might just add some height to your portfolio.
    
        3. **Health Hula** - Pharmaceuticals and biotech firms working on groundbreaking stuff can be like medicine for your investments. Just remember, there's always a bit of a twirl and spin with these.
    
        4. **Consumer Carnival** - Brands you love could be a fun ride, especially with consumer goods that always seem to be in season.
    
        5. **Financial Fiesta** - Banks or fintech companies can be like salsa on your stock tacos—adding a bit of spice and zing!
    
        Remember, always research like you're planning the perfect vacation and balance your choices like you balance a pizza with just the right amount of toppings. And of course, consult a real-world financial oracle before making any big moves. Bon appétit in the stock market buffet!


