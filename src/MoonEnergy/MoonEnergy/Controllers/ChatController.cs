using System.ClientModel;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoonEnergy.ChatTools;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;

namespace MoonEnergy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IEnumerable<IChatTool> _chatTools;

    //private static Dictionary<Guid, ChatHistory> _sessions;
    //private IFunctionHandler _functionHandler;
    private readonly OpenAiConfig _config;
    private static Dictionary<Guid, List<ChatMessage>> _sessions = new();

    public ChatController(IOptions<OpenAiConfig> config, IEnumerable<IChatTool> chatTools)
    {
        _chatTools = chatTools;
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
        string GetToolCallContent(ChatToolCall toolCall)
        {
            foreach (var chatTool in _chatTools)
            {
                if (toolCall.FunctionName == chatTool.Name)
                {
                    return chatTool.Call(toolCall);
                }
            }

            // Handle unexpected tool calls
            throw new NotImplementedException();
        }

        var chatClient = new ChatClient("gpt-4o", _config.Key);
        
        var chatOptions = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 150
        };
        
        var chatTools = _chatTools.Select(tool => tool.Get()).ToList();
        foreach (var tool in chatTools)
        {
            chatOptions.Tools.Add(tool);
        }

        var systemMessage = new SystemChatMessage("You are a useful assistant that replies using a funny style.");
        var userQ = new UserChatMessage(message);
        var messages = new List<ChatMessage>
        {
            systemMessage,
            userQ
        };

        var completion = await chatClient.CompleteChatAsync(messages, chatOptions);

        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            // Add a new assistant message to the conversation history that includes the tool calls
            messages.Add(new AssistantChatMessage(completion));

            foreach (ChatToolCall toolCall in completion.Value.ToolCalls)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, GetToolCallContent(toolCall)));
            }

            completion = await chatClient.CompleteChatAsync(messages, chatOptions);
        }

        Console.WriteLine(completion.Value.Content);

        return Ok(completion.Value.Content[0].Text);
    }
}