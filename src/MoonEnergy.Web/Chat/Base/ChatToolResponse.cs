namespace MoonEnergy.Chat.Base;

public class ChatToolResponse
{
    public required ChatActionType ActionType { get; init; }
    public required string Name { get; init; }
    public required string Text { get; init; }
    public required string Json { get; init; }
}