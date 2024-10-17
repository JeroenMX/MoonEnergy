using System.Text.Json;
using MoonEnergy.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Chat.ChatTools;

public class LoginTool : IChatTool
{
    public string Name => nameof(LoginTool);

    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description(@"
Lets the user login if they are not already or logout if they are logged in. 
Mention the functionality they want to use requires them to be logged in.
Mention they will be redirected shortly to complete the login procedure. 
")
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
        return new ChatToolResponse
        {
            ActionType = ChatActionType.Login,
            Name = Name,
            Text = "Redirecting to the login page",
            Json = JsonSerializer.Serialize(new { })
        };
    }
}