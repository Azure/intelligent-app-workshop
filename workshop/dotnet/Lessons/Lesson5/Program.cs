using Core.Utilities.Config;
using Core.Utilities.Plugins;
using Core.Utilities.Services;
using Core.Utilities.Extensions;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Text;

// Create TracerProvider that exports to console and OTLP
// Following Python Agent Framework pattern: uses gRPC on port 4317
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("agent-telemetry-source")
    .AddConsoleExporter()
    .AddOtlpExporter(options =>
    {
        // Primary: gRPC on 4317 (matches Python Agent Framework)
        options.Endpoint = new Uri("http://localhost:4317");
        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    })
    .Build();

Console.WriteLine("=== Investment Portfolio Analyzer with Sequential Orchestration & OpenTelemetry ===");
Console.WriteLine("This demonstrates MAF Sequential Orchestration with comprehensive telemetry:");
Console.WriteLine("  • Three specialized agents with distributed tracing");
Console.WriteLine("  • OpenTelemetry traces with console and OTLP export");
Console.WriteLine("  • Microsoft Agent Framework instrumentation");
Console.WriteLine("  • AI Toolkit for VS Code integration");
Console.WriteLine();
Console.WriteLine("🔍 OpenTelemetry TracerProvider configured with:");
Console.WriteLine("   • Console Exporter: ✅ Enabled");
Console.WriteLine("   • OTLP Exporter: ✅ http://localhost:4317 (gRPC)");
Console.WriteLine("   • Protocol: gRPC (matching Python Agent Framework)");
Console.WriteLine("   • AI Toolkit: Use tracing view in VS Code");
Console.WriteLine();
Console.WriteLine("To view telemetry data:");
Console.WriteLine("  1. Traces are automatically shown in console output");
Console.WriteLine("  2. OTLP data exported to localhost:4317 (gRPC)");
Console.WriteLine("  3. Open AI Toolkit in VS Code for trace visualization");
Console.WriteLine("  4. Or run an OTEL collector for advanced visualization");
Console.WriteLine();
Console.WriteLine("Enter stock symbols separated by commas (e.g., 'MSFT, AAPL, TSLA, NVDA')");
Console.WriteLine("Type 'quit' to exit.");
Console.WriteLine("====================================================================");
Console.WriteLine();

// Initialize the chat client with Agent Framework  
IChatClient chatClient = AgentFrameworkProvider.CreateChatClientWithApiKey();

// Initialize plugins
TimeInformationPlugin timePlugin = new();
HttpClient httpClient = new();
StockDataPlugin stockDataPlugin = new(new StocksService(httpClient));
HostedWebSearchTool webSearchTool = new();

// Create AI Functions from plugins
var timeTool = AIFunctionFactory.Create(timePlugin.GetCurrentUtcTime);
var stockPriceTool = AIFunctionFactory.Create(stockDataPlugin.GetStockPrice);
var stockPriceDateTool = AIFunctionFactory.Create(stockDataPlugin.GetStockPriceForDate);

// Agent 1: Portfolio Research Agent - Gathers data on all stocks
string researchAgentInstructions = """
    You are a Portfolio Research Agent. Your job is to gather comprehensive market data for stocks.
    
    For each stock symbol provided:
    - Get the current stock price
    - Search the web for recent news and market sentiment
    - Provide a brief summary of each stock's current situation
    
    Provide your complete research in a SINGLE response with clear sections for each stock.
    Format your response as a research report with stock symbols as headers.
    """;

AIAgent researchAgent = new ChatClientAgent(
    chatClient,
    instructions: researchAgentInstructions,
    name: "PortfolioResearchAgent",
    description: "Gathers market data and news for portfolio stocks",
    tools: [stockPriceTool, webSearchTool, timeTool]
)
.AsBuilder()
.UseOpenTelemetry(sourceName: "agent-telemetry-source")
.Build();

// Agent 2: Risk Assessment Agent - Analyzes portfolio risk
string riskAgentInstructions = """
    You are a Risk Assessment Agent. Analyze the portfolio composition and risk profile.
    
    Based on the research provided:
    - Identify sector concentration (tech-heavy, diversified, etc.)
    - Assess portfolio balance and diversification
    - Calculate a risk score from 1-10 (1=very safe, 10=very risky)
    - Highlight any concerns about over-concentration
    
    Provide your complete analysis in a SINGLE response.
    Be concise and actionable.
    """;

AIAgent riskAgent = new ChatClientAgent(
    chatClient,
    instructions: riskAgentInstructions,
    name: "RiskAssessmentAgent",
    description: "Analyzes portfolio risk and diversification"
)
.AsBuilder()
.UseOpenTelemetry(sourceName: "agent-telemetry-source")
.Build();

// Agent 3: Investment Advisor Agent - Provides recommendations
string advisorAgentInstructions = """
    You are an Investment Advisor Agent. Synthesize research and risk analysis into actionable recommendations.
    
    Based on the research and risk assessment:
    - Provide an overall portfolio health score (1-10)
    - Give specific buy/hold/sell recommendations for each stock
    - Suggest rebalancing actions if needed
    - Provide 2-3 key takeaways
    
    Provide your complete recommendations in a SINGLE response.
    Be clear, concise, and actionable.
    """;

AIAgent advisorAgent = new ChatClientAgent(
    chatClient,
    instructions: advisorAgentInstructions,
    name: "InvestmentAdvisorAgent",
    description: "Provides investment recommendations based on research and risk analysis"
)
.AsBuilder()
.UseOpenTelemetry(sourceName: "agent-telemetry-source")
.Build();

// Execute program
const string terminationPhrase = "quit";
string? userInput;

do
{
    Console.Write("Enter portfolio > ");
    userInput = Console.ReadLine();

    if (userInput is not null and not terminationPhrase)
    {
        try
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("PORTFOLIO ANALYSIS - SEQUENTIAL ORCHESTRATION WITH TELEMETRY");
            Console.WriteLine(new string('=', 70) + "\n");
            
            // Build the workflow and convert it to an agent with telemetry
            AIAgent workflowAgent = (await AgentWorkflowBuilder.BuildSequential([
                researchAgent,
                riskAgent,
                advisorAgent
            ]).AsAgentAsync())
            .AsBuilder()
            .UseOpenTelemetry(sourceName: "agent-telemetry-source")
            .Build();
            
            // Run the workflow with streaming output
            string? lastAgentName = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            await foreach (var update in workflowAgent.RunStreamingAsync($"Analyze this portfolio of stocks: {userInput}"))
            {
                // Print header when we see a new agent starting
                if (lastAgentName != update.AuthorName)
                {
                    if (lastAgentName != null)
                    {
                        Console.WriteLine(); // Add spacing between agents
                        Console.WriteLine(new string('-', 70));
                        Console.WriteLine();
                    }
                    
                    lastAgentName = update.AuthorName;
                    Console.WriteLine($"[{update.AuthorName}]");
                    Console.WriteLine(new string('-', 70));
                }
                
                // Stream the text output in real-time
                Console.Write(update.Text);
            }
            
            stopwatch.Stop();
            
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine($"✓ ANALYSIS COMPLETE - Duration: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("✓ OpenTelemetry traces exported to console and OTLP endpoint");
            Console.WriteLine(new string('=', 70));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing portfolio: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        
        Console.WriteLine();
    }
}
while (userInput != terminationPhrase);

Console.WriteLine("Thank you for using the Portfolio Analyzer!");
Console.WriteLine("OpenTelemetry traces have been exported. Check AI Toolkit in VS Code or your observability platform for detailed insights.");
