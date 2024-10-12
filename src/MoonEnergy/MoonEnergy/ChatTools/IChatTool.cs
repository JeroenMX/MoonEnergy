using OpenAI.Chat;

namespace MoonEnergy.ChatTools;

public interface IChatTool
{
    public string Name { get; }
    ChatTool Get();
    string Call(ChatToolCall chatToolCall);
}