using System.Text.Json;
using System.Text.Json.Serialization;
using OpenAI.Chat;

namespace MoonEnergy.ChatTools;

public class WeatherTool : IChatTool
{
    public string Name => nameof(WeatherTool);
    public ChatTool Get()
    {
        var weatherFunctionParameters = new WeatherFunctionParameters();
        var parametersJson = JsonSerializer.Serialize(weatherFunctionParameters);

        return ChatTool.CreateFunctionTool(
            functionName: Name,
            functionDescription: "Get the current weather in a given location",
            functionParameters: BinaryData.FromString(parametersJson)
        );
    }

    public string Call(ChatToolCall chatToolCall)
    {
        // Validate arguments before using them; it's not always guaranteed to be valid JSON!

        try
        {
            using JsonDocument argumentsDocument = JsonDocument.Parse(chatToolCall.FunctionArguments);
            if (!argumentsDocument.RootElement.TryGetProperty("location", out JsonElement locationElement))
            {
                // Handle missing required "location" argument
            }
            else
            {
                string location = locationElement.GetString();
                if (argumentsDocument.RootElement.TryGetProperty("unit", out JsonElement unitElement))
                {
                    return GetCurrentWeather(location, unitElement.GetString());
                }
                
                return GetCurrentWeather(location);
            }
        }
        catch (JsonException)
        {
            // Handle the JsonException (bad arguments) here
        }

        throw new Exception("Oops!");
    }

    private static string GetCurrentWeather(string location, string unit = "celsius")
    {
        // Call the weather API here.
        
        Random random = new Random();
        
        // Generate a random temperature in Kelvin between 50K and 5000K
        int temperature = random.Next(-50, 50);
        
        return $"{temperature} {unit}";
    }
}

public class WeatherFunctionParameters
{
    [JsonPropertyName("type")] public string Type { get; } = "object";

    [JsonPropertyName("properties")] public WeatherProperties Properties { get; set; } = new WeatherProperties();

    [JsonPropertyName("required")] public string[] Required { get; } = new[] { "location" };
}

public class WeatherProperties
{
    [JsonPropertyName("location")] public LocationProperty Location { get; set; } = new LocationProperty();

    [JsonPropertyName("unit")] public UnitProperty Unit { get; set; } = new UnitProperty();
}

public class LocationProperty
{
    [JsonPropertyName("type")] public string Type { get; } = "string";

    [JsonPropertyName("description")] public string Description { get; } = "The city and state, e.g. Boston, MA";
}

public class UnitProperty
{
    [JsonPropertyName("type")] public string Type { get; } = "string";

    [JsonPropertyName("enum")] public string[] Enum { get; } = new[] { "celsius", "fahrenheit" };

    [JsonPropertyName("description")]
    public string Description { get; } = "The temperature unit to use. Infer this from the specified location.";
}