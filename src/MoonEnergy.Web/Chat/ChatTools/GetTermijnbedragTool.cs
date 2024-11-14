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
    
    public ChatToolResponse Call(ChatToolCall chatToolCall, SessionState sessionState)
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
        
        if (sessionState.UserState == null)
        {
            return new ChatToolResponse { Name = Name, Text = "The user is nog logged in." };
        }
        
        return GetTermijnbedrag(sessionState.UserState);
    }
    
    class TermijnbedagParameters
    {
        public string? Klantnummer { get; set; }
        public string? PostcodeHuisnummer { get; set; }
    }

    private ChatToolResponse GetTermijnbedrag(UserState userState)
    {
        var actual = userState.Electricity.InstallmentAmountCurrent;
        var ideal = userState.Electricity.InstallmentAmountIdeal;
        var min = (int)Math.Round((double)userState.Electricity.InstallmentAmountIdeal / 2, 0);
        var max = userState.Electricity.InstallmentAmountIdeal + min;
        
        string message = actual switch
        {
            _ when actual < ideal => "Het termijnbedrag is te laag en het advies is daarom om het te verhogen naar minimaal het ideale bedrag.",
            _ when actual == ideal => "Het termijnbedrag is precies goed.",
            _ when actual > ideal => "Het termijnbedrag is te hoog en het advies is daarom om het te verlagen naar het ideale bedrag.",
            _ => throw new InvalidOperationException("Unexpected comparison result.")
        };
        
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