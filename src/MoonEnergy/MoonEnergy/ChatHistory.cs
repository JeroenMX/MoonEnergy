namespace MoonEnergy;

public class ChatHistory
{
    public List<string> UserMesages { get; set; } = new();
    public List<string> SystemMesages { get; set; } = new();
}