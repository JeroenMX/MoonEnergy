using OpenAI.Chat;

namespace MoonEnergy.Controllers.Chat.Base;

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
    public string? ActionContentAsJson { get; init; }
}