# Creating the Backend API

These changes are already available in the repository. These instructions walk
you through the process followed to create the backend API from the Console application:

1. Start by creating a new directory:

    ```bash
    mkdir -p workshop/dotnet/App/backend
    ```

1. Next create a new SDK .NET project:

    ```bash
    cd workshop/dotnet/App/
    dotnet new webapi -n backend --no-openapi --force
    cd backend
    ```

1. Build project to confirm it is successful:

    ```txt
    dotnet build

    Build succeeded.
        0 Warning(s)
        0 Error(s)
    ```

1. Add the following nuget packages:

    ```bash
    dotnet add package Microsoft.AspNetCore.Mvc
    dotnet add package Swashbuckle.AspNetCore
    ```

1. Replace the contents of `Program.cs` in the project directory with the following code. This file initializes and loads the required services and configuration for the API, namely configuring CORS protection, enabling controllers for the API and exposing Swagger document:

    ```csharp
    using Microsoft.AspNetCore.Antiforgery;
    using Extensions;
    using System.Text.Json.Serialization;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // See: https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    // Required to generate enumeration values in Swagger doc
    builder.Services.AddControllersWithViews().AddJsonOptions(options => 
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddOutputCache();
    builder.Services.AddAntiforgery(options => { 
        options.HeaderName = "X-CSRF-TOKEN-HEADER"; 
        options.FormFieldName = "X-CSRF-TOKEN-FORM"; });
    builder.Services.AddHttpClient();
    builder.Services.AddDistributedMemoryCache();
    // Add Semantic Kernel services
    builder.Services.AddSkServices();

    // Load user secrets
    builder.Configuration.AddUserSecrets<Program>();

    var app = builder.Build();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseOutputCache();
    app.UseRouting();
    app.UseCors();
    app.UseAntiforgery();
    app.MapControllers();

    app.Use(next => context =>
    {
        var antiforgery = app.Services.GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens?.RequestToken ?? string.Empty, new CookieOptions() { HttpOnly = false });
        return next(context);
    });

    app.Map("/", () => Results.Redirect("/swagger"));

    app.MapControllerRoute(
        "default",
        "{controller=ChatController}");

    app.Run();
    ```

1. Next we need to create `Extensions` directory to and add service extensions:

    ```bash
    mkdir Extensions
    cd Extensions
    ```

1. In the `Extensions` directory create a `ServiceExtensions.cs` class with the following code to initialize the semantic kernel:

    ```csharp
    using Core.Utilities.Config;
    using Core.Utilities.Models;
    // Add import for Plugins
    using Core.Utilities.Plugins;
    // Add import required for StockService
    using Core.Utilities.Services;
    using Microsoft.SemanticKernel;

    namespace Extensions;

    public static class ServiceExtensions
    {
        public static void AddSkServices(this IServiceCollection services) 
        {
            services.AddSingleton<Kernel>(_ => 
            {
                IKernelBuilder builder = KernelBuilderProvider.CreateKernelWithChatCompletion();
                // Enable tracing
                builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));
                Kernel kernel = builder.Build();

                // Step 2 - Initialize Time plugin and registration in the kernel
                kernel.Plugins.AddFromObject(new TimeInformationPlugin());

                // Step 6 - Initialize Stock Data Plugin and register it in the kernel
                HttpClient httpClient = new();
                StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
                kernel.Plugins.AddFromObject(stockDataPlugin);

                return kernel;
            });
        }

    }
    ```

1. Next we need to create a `Controllers` directory to add REST API controller classes:

    ```bash
    cd ..
    mkdir Controllers
    cd Controllers
    ```

1. Within the `Controllers` directory create a `ChatController.cs` file which exposes a reply method mapped to the `chat` path and the `HTTP POST` method:

    ```csharp
    using Core.Utilities.Models;
    using Core.Utilities.Extensions;
    // Add import required for StockService
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    // Add ChatCompletion import
    using Microsoft.SemanticKernel.ChatCompletion;
    // Temporarily added to enable Semantic Kernel tracing
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Microsoft.AspNetCore.Mvc;

    namespace Controllers;

    [ApiController]
    [Route("sk")]
    public class ChatController : ControllerBase {

        private readonly Kernel _kernel;
        private readonly OpenAIPromptExecutionSettings _promptExecutionSettings;

        public ChatController(Kernel kernel)
        {
            _kernel = kernel;
            _promptExecutionSettings = new()
            {
                // Step 3 - Add Auto invoke kernel functions as the tool call behavior
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

        }

        [HttpPost("/chat")]
        public async Task<ChatResponse> ReplyAsync([FromBody]ChatRequest request)
        {
            // Get chatCompletionService and initialize chatHistory wiht system prompt
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            if (request.MessageHistory.Count == 0) { 
                chatHistory.AddSystemMessage("You are a friendly financial advisor that only emits financial advice in a creative and funny tone");
            }
            else {
                chatHistory = request.ToChatHistory();
            }

            // Initialize fullMessage variable and add user input to chat history
            string fullMessage = "";
            if (request.InputMessage != null)
            {
                chatHistory.AddUserMessage(request.InputMessage);

                // Step 4 - Provide promptExecutionSettings and kernel arguments
                await foreach (var chatUpdate in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, _promptExecutionSettings, _kernel))
                {
                    Console.Write(chatUpdate.Content);
                    fullMessage += chatUpdate.Content ?? "";
                }
                chatHistory.AddAssistantMessage(fullMessage);
            }
            var chatResponse = new ChatResponse(fullMessage, chatHistory.FromChatHistory());    
            return chatResponse;
        }


    }
    ```

1. Within the `Controllers` directory create a `PluginInfoController.cs` file which exposes a method mapped to the `/puginInfo/metadata` path and the `HTTP GET` method to print out all plugin information loaded in the kernel:

    ```csharp
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.SemanticKernel;
    using Microsoft.AspNetCore.Mvc;
    using Core.Utilities.Models;
    using Core.Utilities.Extensions;

    namespace Controllers;

    [ApiController]
    [Route("sk")]
    public class PluginInfoController : ControllerBase {

        private readonly Kernel _kernel;
        
        public PluginInfoController(Kernel kernel)
        {
            _kernel = kernel;
        }

        /// <summary>
        /// Get the metadata for all the plugins and functions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/puginInfo/metadata")]
        public async Task<IList<PluginFunctionMetadata>> GetPluginInfoMetadata()
        {
            var functions = _kernel.Plugins.GetFunctionsMetadata().ToPluginFunctionMetadataList();
            return functions;
        }
    }
    ```

## Running the Backend API locally

1. To run API locally first copy valid `appsettings.json` from completed `Lessons/Lesson3` into `backend` directory:

    ```bash
    #cd into backend directory
    cd ../
    cp ../../Lessons/Lesson3/appsettings.json .
    ```

1. Next run using `dotnet run`:

    ```bash
    dotnet run
    ...
    info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
    ```

1. Application will start on specified port (port may be different). Navigate to <http://localhost:5000> or corresponding [forwarded address](https://docs.github.com/en/codespaces/developing-in-a-codespace/forwarding-ports-in-your-codespace) (if using Github CodeSpace) and it should redirect you to the swagger UI page.

1. You can either test the `chat` endpoint using the "Try it out" feature from within Swagger UI, or via command line using `curl` command:

    ```bash
    curl -X 'POST' \
    'http://localhost:5000/chat' \
    -H 'accept: text/plain' \
    -H 'Content-Type: application/json' \
    -d '{
    "inputMessage": "what is Microsoft price?",
    "messageHistory": [
    ]
    }'
    ```

1. You can also test the `pluginInfo/metadata` endpoint using the "Try it out" feature from within Swagger UI, or via command line using `curl` command:

    ```bash
    curl -X 'GET' \
    'http://localhost:5000/pluginInfo/metadata' \
    -H 'accept: text/plain'
    ```
