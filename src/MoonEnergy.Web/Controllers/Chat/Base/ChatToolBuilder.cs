using System.Text.Json;
using OpenAI.Chat;

namespace MoonEnergy.Controllers.Chat.Base;

public class ChatToolBuilder
{
    private string? _name;
    private string? _description;
    private readonly FunctionParameterBuilder _parameterBuilder = new FunctionParameterBuilder();

    public ChatToolBuilder Name(string name)
    {
        _name = name;
        return this;
    }

    public ChatToolBuilder Description(string description)
    {
        _description = description;
        return this;
    }

    public ChatToolBuilder AddParameter(string name, Action<PropertyBuilder> propertyAction)
    {
        _parameterBuilder.AddProperty(name, propertyAction);
        return this;
    }

    public ChatTool Build()
    {
        if (string.IsNullOrEmpty(_name))
            throw new InvalidOperationException("Function name is required.");

        return ChatTool.CreateFunctionTool(
            functionName: _name,
            functionDescription: _description,
            functionParameters: BinaryData.FromString(_parameterBuilder.Build())
        );
    }
}

public class FunctionParameterBuilder
{
    private readonly Dictionary<string, object> _properties = new();
    private readonly List<string> _required = new();

    public FunctionParameterBuilder AddProperty(string name, Action<PropertyBuilder> propertyAction)
    {
        var propertyBuilder = new PropertyBuilder(name, _required);
        propertyAction(propertyBuilder);
        _properties[name] = propertyBuilder.Build();
        return this;
    }

    public string Build()
    {
        var result = new
        {
            type = "object",
            properties = _properties,
            required = _required.Count > 0 ? _required : null
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}

public class PropertyBuilder
{
    private readonly Dictionary<string, object> _property = new();
    private readonly string _name;
    private readonly List<string> _requiredList;

    public PropertyBuilder(string name, List<string> requiredList)
    {
        _name = name;
        _requiredList = requiredList;
    }

    public PropertyBuilder Type(string type)
    {
        _property["type"] = type;
        return this;
    }

    public PropertyBuilder Description(string description)
    {
        _property["description"] = description;
        return this;
    }

    public PropertyBuilder Enum(params string[] values)
    {
        _property["enum"] = values;
        return this;
    }

    public PropertyBuilder Required()
    {
        _requiredList.Add(_name);
        return this;
    }

    internal Dictionary<string, object> Build() => _property;
}