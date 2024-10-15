using OpenAI.Chat;

namespace MoonEnergy.Controllers.Chat.Base;

public interface IChatTool
{
    public string Name { get; }
    ChatTool Get();
    ChatToolResponse Call(ChatToolCall chatToolCall);
}