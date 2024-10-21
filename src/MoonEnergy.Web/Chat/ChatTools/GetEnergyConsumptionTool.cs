using System.Text.Json;
using MoonEnergy.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Chat.ChatTools;

public class GetEnergyConsumptionTool : IChatTool
{
    public string Name => nameof(GetEnergyConsumptionTool);

    public ChatTool Get()
    {
        var tool = new ChatToolBuilder()
            .Name(Name)
            .Description(@"
Retrieve the energy consumption for the customer for a period of time.
To use this tool the user has to be logged in.
")
            .AddParameter(nameof(Parameters.period), p => p
                .Type("string")
                .Enum("thisyear", "thismonth")
                .Description("The period over which the custom energy consumption is.")
                .Required())
            .Build();

        return tool;
    }

    public ChatToolResponse Call(ChatToolCall chatToolCall, UserState? userState)
    {
        // Validate arguments before using them; it's not always guaranteed to be valid JSON!

        var parameters =
            JsonParameterExtractor.ExtractParameters<Parameters>(chatToolCall.FunctionArguments);

        if (parameters.period == null)
        {
            return new ChatToolResponse { Name = Name, Text = $"{nameof(parameters.period)} is required" };
        }

        if (userState == null)
        {
            return new ChatToolResponse { Name = Name, Text = "The user is nog logged in." };
        }

        return GetConsumption(userState, parameters.period);
    }

    class Parameters
    {
        public string? period { get; set; }
    }

    private ChatToolResponse GetConsumption(UserState userState, string period)
    {
        if (period == "thismonth")
        {
            return new ChatToolResponse
            {
                ActionType = ChatActionType.Render,
                Name = Name,
                Text = $"The user's consumption for this month.  {userState.Electricity.ThisMonthUsage.Sum()} kWh.",
                Json = JsonSerializer.Serialize(new { period = period, usage = userState.Electricity.ThisMonthUsage })
            };
        }

        if (period == "thisyear")
        {
            return new ChatToolResponse
            {
                ActionType = ChatActionType.Render,
                Name = Name,
                Text = $"The user's consumption for this year. {userState.Electricity.MontlyUsage.Sum()} kWh.",
                Json = JsonSerializer.Serialize(new { period = period, usage = userState.Electricity.MontlyUsage })
            };
        }

        throw new NotSupportedException($"{period} is not supported.");
    }
}