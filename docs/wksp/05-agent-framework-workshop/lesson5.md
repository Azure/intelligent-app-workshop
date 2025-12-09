# Lesson 5: Multi-Agent Orchestration with OpenTelemetry

This lesson demonstrates sequential orchestration where multiple specialized agents work together in a defined sequence to analyze a complete investment portfolio, while implementing comprehensive OpenTelemetry tracing for observability.

## Prerequisites

1. **Install AI Toolkit for VS Code**:
    * Open Visual Studio Code
    * Go to Extensions (Ctrl+Shift+X)
    * Search for "AI Toolkit"
    * Install the **AI Toolkit** extension by Microsoft
    * Restart VS Code if needed

1. **Configure AI Toolkit Tracing**:
    * Open the **AI Toolkit** panel in VS Code
    * Navigate to the **Agent and Workflow Tools** section, then click on **Tracing**
    * Click **Start Collector** to start the local OTLP trace collector
    * The collector will start listening on `http://localhost:4317` (gRPC) and `http://localhost:4318` (HTTP)

## Getting Started

1. Switch to Lesson 5 directory:

    ```bash
    cd workshop/dotnet/Lessons/Lesson5
    ```

1. Copy the configuration file from the Solutions directory:

    ```powershell
    cp ../Lesson1/appsettings.json appSettings.json
    ```

## Testing with OpenTelemetry

1. **Ensure AI Toolkit is running**:
    * Check that the OTLP collector is started in VS Code AI Toolkit
    * Verify it's listening on `localhost:4317`

1. **Run the enhanced application**:

    ```bash
    dotnet run
    ```

1. **Test with portfolio analysis**:
    * Enter stock symbols: `MSFT, AAPL, NVDA`
    * Observe comprehensive telemetry output in console
    * Watch for OpenTelemetry activity traces with timing information

1. **View traces in AI Toolkit**:
    * In VS Code, open AI Toolkit panel
    * Navigate to Tracing section
    * Click "Refresh" to see new traces
    * Select a trace to explore the span tree
    * Review Input + Output tab for AI message flows
    * Check Metadata tab for detailed trace information

## Expected Behavior with OpenTelemetry

You should observe:

1. **Sequential Agent Execution**:
    * Portfolio Research Agent gathers market data and news
    * Risk Assessment Agent analyzes portfolio composition and risk
    * Investment Advisor Agent provides recommendations

2. **Comprehensive Telemetry Data**:
    * Console output shows OpenTelemetry activities with IDs and timing
    * Each agent's execution is traced with performance metrics
    * Complete workflow orchestration visibility

3. **AI Toolkit Integration**:
    * Traces appear in VS Code AI Toolkit tracing view
    * Span hierarchy shows workflow → individual agents
    * Detailed metadata about model calls, token usage, and response IDs

4. **Performance Monitoring**:
    * Total execution time displayed
    * Individual agent timing visible in traces
    * Memory and resource usage tracking

This enhanced lesson demonstrates how to build production-ready multi-agent systems with enterprise-grade observability using OpenTelemetry and the AI Toolkit for VS Code.
