namespace MoonEnergy.Controllers.Chat.Base;

public record ChatToolResponse(ChatActionType ActionType, string Text, string Json);