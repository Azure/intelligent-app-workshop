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
                string role = chatMessage.Role;
                if ("Tool".Equals(role, StringComparison.OrdinalIgnoreCase)) {
                    role = AuthorRole.Assistant.Label;
                    role = "assistant";
                }
                chatHistory.Add(new ChatMessageContent(new AuthorRole(role), chatMessage.Message));
            });
            return chatHistory;
        }
 
        public static List<ChatMessage> FromChatHistory(this ChatHistory chatHistory) {
            var messageHistory = new List<ChatMessage>();
            messageHistory.AddRange(chatHistory
                .Where(m => m.Content != null)
                .Select(m => new ChatMessage(m.Content!, m.Role.Label)));

            return messageHistory;
        }

    }
}
