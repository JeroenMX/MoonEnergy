using System.Text.Json;
using MoonEnergy.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Chat.ChatTools;

public class SetTermijnbedragTool : IChatTool
{
    public string Name => nameof(SetTermijnbedragTool);

    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description(@"
Wijzig het huidige termijnbedrag. Om deze tool te gebruiken moet de gebruiker ingelogd zijn.
")
            .AddParameter("klantnummer", p => p
                .Type("string")
                .Description("Het klantnummer van de klant")
                .Required())
            .AddParameter("postcodeHuisnummer", p => p
                .Type("string")
                .Description("De postcode en het huisnummer van de klant")
                .Required())
            .AddParameter("termijnbedrag", p => p
                .Type("string")
                .Description("Het nieuwe termijnbedrag van de klant in hele euros")
                .Required())
            .Build();

        return tool;
    }

    public ChatToolResponse Call(ChatToolCall chatToolCall, SessionState sessionState)
    {
        // Validate arguments before using them; it's not always guaranteed to be valid JSON!

        var parameters =
            JsonParameterExtractor.ExtractParameters<TermijnbedagParameters>(chatToolCall.FunctionArguments);

        if (parameters.Klantnummer == null)
        {
            throw new ArgumentException($"{nameof(parameters.Klantnummer)} is required");
        }

        if (parameters.PostcodeHuisnummer == null)
        {
            throw new ArgumentException($"{nameof(parameters.PostcodeHuisnummer)} is required");
        }

        if (parameters.Termijnbedrag == null)
        {
            throw new ArgumentException($"{nameof(parameters.Termijnbedrag)} is required");
        }
        
        if (sessionState.UserState == null)
        {
            return new ChatToolResponse { Name = Name, Text = "The user is nog logged in." };
        }

        return SetTermijnbedrag(parameters.Termijnbedrag, sessionState.UserState);
    }

    class TermijnbedagParameters
    {
        public string? Klantnummer { get; set; }
        public string? PostcodeHuisnummer { get; set; }
        public string? Termijnbedrag { get; set; }
    }

    private ChatToolResponse SetTermijnbedrag(string termijnbedrag, UserState userState)
    {
        var tb = int.Parse(termijnbedrag);

        if (tb < 60)
        {
            return new ChatToolResponse
            {
                ActionType = ChatActionType.Render,
                Name = Name,
                Text = "Het termijnbedrag mag niet lager zijn dan 60 euro",
                Json = JsonSerializer.Serialize(new { })
            };
        }
        else if (tb > 100)
        {
            return new ChatToolResponse
            {
                ActionType = ChatActionType.Render,
                Name = Name,
                Text = "Het termijnbedrag mag niet hoger zijn dan 100 euro",
                Json = JsonSerializer.Serialize(new { })
            };
        }

        userState.Electricity.InstallmentAmountCurrent = tb;

        return new ChatToolResponse
        {
            ActionType = ChatActionType.Render,
            Name = Name,
            Text = $"Het termijnbedrag is aangepast naar {termijnbedrag} euro",
            Json = JsonSerializer.Serialize(new { })
        };
    }
}