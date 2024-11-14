using OpenAI.Chat;

namespace MoonEnergy.Chat.Base;

public class ChatSession
{
    public SessionState SessionState { get; set; }
    public List<ChatInteraction> Interactions { get; } = new();
    
    public ChatSession(string sessionId)
    {
        SessionState = new SessionState(sessionId);
    }
}

public class ChatInteraction
{
    public required List<ChatAction> Actions { get; init; }  
    public required List<ChatMessage> Messages { get; init; }
}

public class ChatAction
{
    public required ChatActionType Action { get; init; }
    public required string Name { get; init; }
    public string? ContentAsJson { get; init; }
}