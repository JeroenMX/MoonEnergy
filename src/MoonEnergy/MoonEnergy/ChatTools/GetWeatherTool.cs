using MoonEnergy.ChatTools.Base;
using OpenAI.Chat;

namespace MoonEnergy.ChatTools;

public class GetWeatherTool : IChatTool
{
    public string Name => nameof(GetWeatherTool);

    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description("Get the current weather in a given location")
            .AddParameter("location", p => p
                .Type("string")
                .Description("The city and state, e.g. San Francisco, CA")
                .Required())
            .AddParameter("unit", p => p
                .Type("string")
                .Enum("celsius", "fahrenheit")
                .Description("The temperature unit to use"))
            .Build();

        return tool;
    }
    
    public string Call(ChatToolCall chatToolCall)
    {
        // Validate arguments before using them; it's not always guaranteed to be valid JSON!

        var parameters = JsonParameterExtractor.ExtractParameters<WeatherParameters>(chatToolCall.FunctionArguments);

        if (parameters.Location == null)
        {
            throw new ArgumentException("Location is required");
        }
        
        return GetCurrentWeather(parameters.Location, parameters.Unit);
    }
    
    class WeatherParameters
    {
        public string? Location { get; set; }
        public string? Unit { get; set; }
    }

    private static string GetCurrentWeather(string location, string? unit)
    {
        unit ??= "celsius";
        
        var random = new Random();
        int temperature = random.Next(-50, 50);
        
        return $"{temperature} {unit}";
    }
}
