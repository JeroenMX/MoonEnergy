using MoonEnergy.ChatTools.Base;
using OpenAI.Chat;

namespace MoonEnergy.ChatTools;

public class GetTermijnbedragTool : IChatTool
{
    public string Name => nameof(GetTermijnbedragTool);
    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description("Haal het huidige termijnbedrag op. Wanneer de klant zijn termijnbedrag wil wijzigen haal je eerst deze gegevens op.")
            .AddParameter("klantnummer", p => p
                .Type("string")
                .Description("Het klantnummer van de klant")
                .Required())
            .AddParameter("postcodeHuisnummer", p => p
                .Type("string")
                .Description("De postcode en het huisnummer van de klant")
                .Required())
            .Build();

        return tool;
    }
    
    public string Call(ChatToolCall chatToolCall)
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

    private static string GetTermijnbedrag(string klantnummer, string postcodeHuisnummer)
    {
        return "actual: 30,00. ideal: 70. minimum: 60. max: 100. feedback: Het termijnbedrag is veel te laag. Dit zou zo snel mogelijk moeten worden aangepast.";
    }
}