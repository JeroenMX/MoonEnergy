using System.Text.Json;

namespace MoonEnergy.Chat.Base;

public static class JsonParameterExtractor
{
    public static T ExtractParameters<T>(BinaryData jsonString)
        where T : new()
    {
        T result = new T();

        using var document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;

        foreach (var prop in typeof(T).GetProperties())
        {
            if (TryGetPropertyCaseInsensitive(root, prop.Name.ToLowerInvariant(), out JsonElement element))
            {
                var value = ExtractValue(element, prop.PropertyType);
                prop.SetValue(result, value);
            }
        }

        return result;
    }
    
    public static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static object? ExtractValue(JsonElement element, Type targetType)
    {
        if (targetType == typeof(string))
            return element.GetString();
        else if (targetType == typeof(int))
            return element.GetInt32();
        else if (targetType == typeof(double))
            return element.GetDouble();
        else if (targetType == typeof(bool))
            return element.GetBoolean();
        // Add more type checks as needed

        return null;
    }
}