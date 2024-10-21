namespace MoonEnergy.Chat.Base;

public class ChatToolResponse
{
    public ChatActionType ActionType { get; init; } = ChatActionType.None;
    public required string Name { get; init; }
    public required string Text { get; init; }
    public string? Json { get; init; }
}
