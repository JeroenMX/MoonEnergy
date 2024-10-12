using System.ClientModel;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace MoonEnergy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    //private static Dictionary<Guid, ChatHistory> _sessions;
    //private IFunctionHandler _functionHandler;
    private readonly OpenAiConfig _config;
    private static Dictionary<Guid, List<ChatMessage>> _sessions = new();

    public ChatController(IOptions<OpenAiConfig> config)
    {
        _config = config.Value;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string message)
    {
        // history.Add(new ChatMessage(ChatMessageRole.User, message));
        //
        // var options = new ChatCompletionsOptions(_config.OpenAi.ChatEngine,
        //     history.Messages)
        // {
        //     Temperature = 0.7f,
        //     MaxTokens = 500,
        // };
        
        var chatClient = new ChatClient("gpt-4o", _config.Key);
        
        var systemMessage = new SystemChatMessage("You are a useful assistant that replies using a funny style.");
        var userQ = new UserChatMessage(message);
        var messages = new List<ChatMessage>
        {
            systemMessage,
            userQ
        };
        
        var response = await chatClient.CompleteChatAsync(messages);
        
        Console.WriteLine(response.Value.Content);

        return Ok(response.Value.Content[0].Text);
    }
}