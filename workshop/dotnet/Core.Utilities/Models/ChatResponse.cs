namespace Core.Utilities.Models;

public record ChatResponse(
    string Response, 
    List<ChatMessage> MessageHistory
);

