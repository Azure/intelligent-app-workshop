using Core.Utilities.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace Core.Utilities.Extensions
{
    public static class ModelExtensionMethods
    {
        public static string FormatStockData(this Stock stockData)
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("| Symbol | Price | Open | Low | High | Date ");
            stringBuilder.AppendLine("| ----- | ----- | ----- | ----- |");
            stringBuilder.AppendLine($"| {stockData.Symbol} | {stockData.Close} | {stockData.Open} | {stockData.Low} | {stockData.High} | {stockData.From} ");

            return stringBuilder.ToString();
        }
 
        public static ChatHistory ToChatHistory(this ChatRequest chatRequest) 
        {
            var chatHistory = new ChatHistory();
            chatRequest.MessageHistory.ForEach(chatMessage => {
                string role = chatMessage.Role.ToString();
                if ("Tool".Equals(role, StringComparison.OrdinalIgnoreCase)) {
                    role = AuthorRole.Assistant.Label;
                }
                chatHistory.Add(new ChatMessageContent(new AuthorRole(role), chatMessage.Message));
            });
            return chatHistory;
        }
 
        public static List<ChatMessage> FromChatHistory(this ChatHistory chatHistory) {
            var messageHistory = new List<ChatMessage>();
            messageHistory.AddRange(chatHistory
                .Where(m => m.Content != null)
                .Select(m => new ChatMessage(m.Content!, ParseRoleFromAuthorRole(m.Role))));

            return messageHistory;
        }
        
        private static Role ParseRoleFromAuthorRole(AuthorRole authorRole)
        {
            return authorRole.Label.ToLowerInvariant() switch
            {
                "system" => Role.System,
                "user" => Role.User,
                "assistant" => Role.Assistant,
                "tool" => Role.Tool,
                _ => Role.User // Default to User for unrecognized roles
            };
        }

        public static IList<PluginFunctionMetadata> ToPluginFunctionMetadataList(this IList<KernelFunctionMetadata> plugins)
        {
            return plugins.Select(p => p.ToPluginFunctionMetadata()).ToList();
        }

        public static PluginFunctionMetadata ToPluginFunctionMetadata(this KernelFunctionMetadata kernelFunctionMetadata)
        {
            return new PluginFunctionMetadata(kernelFunctionMetadata.Name, 
                kernelFunctionMetadata.Description, 
                kernelFunctionMetadata.Parameters.Select(p => p.ToPluginParameterMetadata()).ToList());
        }

        public static PluginParameterMetadata ToPluginParameterMetadata(this KernelParameterMetadata pluginParameterMetadata)
        {
            return new PluginParameterMetadata(pluginParameterMetadata.Name, pluginParameterMetadata.ParameterType?.Name ?? string.Empty, pluginParameterMetadata.Description, pluginParameterMetadata.DefaultValue);
        }

        public static String ToPrintableString(this IList<KernelFunctionMetadata> plugins) {
            var pluginFunctionMetadataList = plugins.ToPluginFunctionMetadataList();
            return pluginFunctionMetadataList?.ToPrintableString() ?? string.Empty;
        }        

        private static String ToPrintableString(this IList<PluginFunctionMetadata> functions)
        {
            var sb = new StringBuilder();
            sb.AppendLine("**********************************************");
            sb.AppendLine("****** Registered plugins and functions ******");
            sb.AppendLine("**********************************************");
            sb.AppendLine();
            foreach (PluginFunctionMetadata func in functions)
            {
                sb.AppendLine(ToString(func));
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Returns the function information as string
        /// </summary>
        /// <param name="func"></param>
        private static String ToString(this PluginFunctionMetadata func)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Plugin: {func.Name}");
            sb.AppendLine($"   {func.Name}: {func.Description}");

            if (func.Parameters.Count > 0)
            {
                sb.AppendLine("      Params:");
                foreach (var p in func.Parameters)
                {
                    sb.AppendLine($"      - {p.Name}: {p.Description}");
                    sb.AppendLine($"        default: '{p.DefaultValue}'");
                }
            }
            return sb.ToString();
        }

    }
}
