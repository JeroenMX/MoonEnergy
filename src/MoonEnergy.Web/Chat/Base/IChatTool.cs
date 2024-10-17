using OpenAI.Chat;

namespace MoonEnergy.Chat.Base;

public interface IChatTool
{
    public string Name { get; }
    ChatTool Get();
    ChatToolResponse Call(ChatToolCall chatToolCall);
}