using OpenAI.Chat;

namespace MoonEnergy.Chat.Base;

public class ChatSession
{
    public bool IsAuthenticated { get; set; } = false;
    public string? Name { get; set; }
    public required string SessionId { get; set; }
    public required List<ChatInteraction> Interactions { get; set; }
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
    public required string ActionContentAsJson { get; init; }
}