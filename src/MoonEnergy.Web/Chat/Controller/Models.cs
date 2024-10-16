namespace MoonEnergy.Chat.Controller;

public class MessageModel
{
    public string Message { get; set; } = null!;
    public string SessionId { get; set; } = null!;
}

public class ChatInteraction
{
    public required List<ChatMessage> Messages { get; init; }
    public required List<ChatAction> Actions { get; init; }
}

public class ChatMessage
{
    public required string Type { get; init; }
    public required string Text { get; init; }
}

public class ChatAction
{
    public required int Action { get; init; }
    public required string Name { get; init; }
    public required string ContentAsJson { get; init; }
}