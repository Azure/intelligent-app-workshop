namespace Core.Utilities.Models;

public record ChatRequest(
    string InputMessage, 
    List<ChatMessage> MessageHistory
);

