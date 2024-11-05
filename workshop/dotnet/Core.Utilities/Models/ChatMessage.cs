namespace Core.Utilities.Models;

public record ChatMessage (
    string Message, 
    Role Role
);

public enum Role {
    User,
    Assistant,
    Tool,
    System
}

