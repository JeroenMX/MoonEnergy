using System.Text.Json;
using MoonEnergy.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Chat.ChatTools;

public class GetTermijnbedragTool : IChatTool
{
    public string Name => nameof(GetTermijnbedragTool);
    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description(@"
Haal het huidige termijnbedrag op. Wanneer de klant zijn termijnbedrag wil wijzigen haal je eerst deze gegevens op. 
Om deze tool te gebruiken moet de gebruiker ingelogd zijn.
")
            .AddParameter(nameof(TermijnbedagParameters.Klantnummer), p => p
                .Type("string")
                .Description("Het klantnummer van de klant")
                .Required())
            .AddParameter(nameof(TermijnbedagParameters.PostcodeHuisnummer), p => p
                .Type("string")
                .Description("De postcode en het huisnummer van de klant")
                .Required())
            .Build();

        return tool;
    }
    
    public ChatToolResponse Call(ChatToolCall chatToolCall, UserState? userState)
    {
        // Validate arguments before using them; it's not always guaranteed to be valid JSON!

        var parameters = JsonParameterExtractor.ExtractParameters<TermijnbedagParameters>(chatToolCall.FunctionArguments);

        if (parameters.Klantnummer == null)
        {
            throw new ArgumentException($"{nameof(parameters.Klantnummer)} is required");
        }
        
        if (parameters.PostcodeHuisnummer == null)
        {
            throw new ArgumentException($"{nameof(parameters.PostcodeHuisnummer)} is required");
        }
        
        return GetTermijnbedrag(parameters.Klantnummer, parameters.PostcodeHuisnummer);
    }
    
    class TermijnbedagParameters
    {
        public string? Klantnummer { get; set; }
        public string? PostcodeHuisnummer { get; set; }
    }

    private ChatToolResponse GetTermijnbedrag(string klantnummer, string postcodeHuisnummer)
    {
        var actual = 110;
        var ideal = 100;
        var min = 40;
        var max = 200;
        var message = "Het termijnbedrag is veel te laag en het advies is daarom om het te verhogen naar minimaal het ideale bedrag.";
        
        var text = $"actual: {actual}. ideal: {ideal}. minimum: {min}. max: {max}. feedback: {message}";
        var json = JsonSerializer.Serialize(new { actual, ideal, min, max, message });

        return new ChatToolResponse
        {
            ActionType = ChatActionType.Render,
            Name = Name,
            Text = text,
            Json = json
        };
    }
}