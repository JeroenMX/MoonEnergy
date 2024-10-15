using System.Text.Json;
using MoonEnergy.Controllers.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Controllers.Chat.ChatTools;

public class LoginTool : IChatTool
{
    public string Name => nameof(LoginTool);

    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description("Lets the user login if he is not already or logout if he is logged in.")
            .AddParameter("action", p => p
                .Type("string")
                .Description("Login or logout")
                .Enum("login", "Logout")
                .Required())
            .Build();

        return tool;
    }

    public ChatToolResponse Call(ChatToolCall chatToolCall)
    {
        return new ChatToolResponse(ChatActionType.Login, "Redirecting to the login page",
            JsonSerializer.Serialize(new { }));
    }
}