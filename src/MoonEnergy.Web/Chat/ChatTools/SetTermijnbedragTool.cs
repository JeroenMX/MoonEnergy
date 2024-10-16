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
            .Description("Wijzig het huidige termijnbedrag.")
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

    public ChatToolResponse Call(ChatToolCall chatToolCall)
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

        return SetTermijnbedrag(parameters.Klantnummer, parameters.PostcodeHuisnummer, parameters.Termijnbedrag);
    }

    class TermijnbedagParameters
    {
        public string? Klantnummer { get; set; }
        public string? PostcodeHuisnummer { get; set; }
        public string? Termijnbedrag { get; set; }
    }

    private static ChatToolResponse SetTermijnbedrag(string klantnummer, string postcodeHuisnummer,
        string termijnbedrag)
    {
        var tb = int.Parse(termijnbedrag);

        if (tb < 60)
        {
            return new ChatToolResponse(ChatActionType.Render, "Het termijnbedrag mag niet lager zijn dan 60 euro",
                JsonSerializer.Serialize(new { }));
        }
        else if (tb > 100)
        {
            return new ChatToolResponse(ChatActionType.Render, "Het termijnbedrag mag niet hoger zijn dan 100 euro",
                JsonSerializer.Serialize(new { }));
        }

        return new ChatToolResponse(ChatActionType.Render, $"Het termijnbedrag is aangepast naar {termijnbedrag} euro",
            JsonSerializer.Serialize(new { }));
    }
}