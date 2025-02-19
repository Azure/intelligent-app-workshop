using Core.Utilities.Models;
using Core.Utilities.Config;
using Core.Utilities.Extensions;
// Add import required for StockService
using Microsoft.SemanticKernel;
// Add ChatCompletion import
using Microsoft.SemanticKernel.ChatCompletion;
// Add import for Agents
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents;
// Temporarily added to enable Semantic Kernel tracing

using Azure.AI.Projects;
using Azure.Identity;

using Microsoft.AspNetCore.Mvc;
using Azure;

namespace Controllers;

[ApiController]
[Route("sk")]
public class ChatController : ControllerBase {

    private readonly Kernel _kernel;

    private AzureAIAgent _stockSentimentAgent;
    private AgentsClient _agentsClient;
    private readonly string _connectionString;
    private readonly string _groundingWithBingConnectionId;
    private readonly string _deploymentName;
    private readonly string _managedIdentityClientId;
    
    public ChatController(Kernel kernel)
    {
        _kernel = kernel;       

        _connectionString = AISettingsProvider.GetSettings().AIFoundryProject.ConnectionString;
        _groundingWithBingConnectionId = AISettingsProvider.GetSettings().AIFoundryProject.GroundingWithBingConnectionId;
        _deploymentName = AISettingsProvider.GetSettings().AIFoundryProject.DeploymentName;
        _managedIdentityClientId = AISettingsProvider.GetSettings().ManagedIdentity.ClientId;
         
        _agentsClient = GetAgentsClient().Result;
        _stockSentimentAgent = GetAzureAIAgent().Result;
    }

    /// <summary>
    /// Get StockSemanticAgent instance
    /// </summary>
    /// <returns></returns>
    private async Task<AzureAIAgent> GetAzureAIAgent()
    {
        var credentialOptions = new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = _managedIdentityClientId
        };
        var projectClient = new AIProjectClient(_connectionString, new DefaultAzureCredential(credentialOptions));
        
        var clientProvider =  AzureAIClientProvider.FromConnectionString(_connectionString, new DefaultAzureCredential(credentialOptions));
                    
        ConnectionResponse bingConnection = await projectClient.GetConnectionsClient().GetConnectionAsync(_groundingWithBingConnectionId);
        var connectionId = bingConnection.Id;

        ToolConnectionList connectionList = new ToolConnectionList
        {
            ConnectionList = { new ToolConnection(connectionId) }
        };
        BingGroundingToolDefinition bingGroundingTool = new BingGroundingToolDefinition(connectionList);

        var definition = await _agentsClient.CreateAgentAsync(
            _deploymentName,
            instructions:
                    """
                    Your responsibility is to find the stock sentiment for a given Stock, emitting advice in a creative and funny tone.

                    RULES:
                    - Report a stock sentiment scale from 1 to 10 where stock sentiment is 1 for sell and 10 for buy.
                    - Only use current data reputable sources such as Yahoo Finance, MarketWatch, Fidelity and similar.
                    - Provide the stock sentiment scale in your response and a recommendation to buy, hold or sell.
                    - Include the reasoning behind your recommendation.
                    - Be sure to cite the source of the information.
                    """,
            tools:
            [
                bingGroundingTool
            ]);
        var agent = new AzureAIAgent(definition, clientProvider)
        {
            Kernel = _kernel,
        };
        
        return agent;
    }
    /// <summary>
    /// Get AgentsClient instance
    /// </summary>
    /// <returns></returns>
    private async Task<AgentsClient> GetAgentsClient()
    {
        var clientProvider =  AzureAIClientProvider.FromConnectionString(_connectionString, new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = _managedIdentityClientId
            }
        ));
        return clientProvider.Client.GetAgentsClient();
    }

    [HttpPost("/chat")]
    public async Task<ChatResponse> ReplyAsync([FromBody]ChatRequest request)
    {
        var chatHistory = new ChatHistory();
        if (request.MessageHistory.Count == 0) { 
            chatHistory.AddSystemMessage("You are a friendly financial advisor.");
        }
        else {
            chatHistory = request.ToChatHistory();
        }

        // Initialize fullMessage variable and add user input to chat history
        string fullMessage = "";
        if (request.InputMessage != null)
        {
            chatHistory.AddUserMessage(request.InputMessage);

            // Create a thread for the agent conversation.
            AgentThread thread = await _agentsClient.CreateThreadAsync();

            ChatMessageContent message = new(AuthorRole.User, request.InputMessage);
            await _stockSentimentAgent.AddChatMessageAsync(thread.Id, message);

            await foreach (ChatMessageContent response in _stockSentimentAgent.InvokeAsync(thread.Id))
            {
                // Include TextContent (via ChatMessageContent.Content), if present.
                string contentExpression = string.IsNullOrWhiteSpace(response.Content) ? string.Empty : response.Content;
                chatHistory.AddAssistantMessage(contentExpression);
                fullMessage += contentExpression;

                // Provide visibility for inner content (that isn't TextContent).
                foreach (KernelContent item in response.Items)
                {
                    if (item is AnnotationContent annotation)
                    {
                        var annotationExpression = ($"  [{item.GetType().Name}] {annotation.Quote}: File #{annotation.FileId}");
                        chatHistory.AddAssistantMessage(annotationExpression);
                        fullMessage += annotationExpression;
                    }
                }
            }
        }
            
        var chatResponse = new ChatResponse(fullMessage, chatHistory.FromChatHistory());    
        return chatResponse;
    }

}