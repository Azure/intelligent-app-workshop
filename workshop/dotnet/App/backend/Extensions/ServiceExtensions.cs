using Core.Utilities.Config;
using Core.Utilities.Models;
// Add import for Plugins
using Core.Utilities.Plugins;
// Add import required for StockService
using Core.Utilities.Services;
using Microsoft.SemanticKernel;
// Add imports for Bing Search plugin
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

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
            
            // Initialize Bing Search plugin
            var bingApiKey = AISettingsProvider.GetSettings().BingSearchService.ApiKey;
            if (!string.IsNullOrEmpty(bingApiKey))
            {
                var bingConnector = new BingConnector(bingApiKey);
                var bing = new WebSearchEnginePlugin(bingConnector);
                kernel.ImportPluginFromObject(bing, "bing");
            }

            return kernel;
        });
    }

}