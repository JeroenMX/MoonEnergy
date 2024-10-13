using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoonEnergy.ChatTools.Base;
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
    private static Dictionary<string, List<ChatMessage>> _sessions = new();

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
    public async Task<IActionResult> Post([FromBody] MessageModel message)
    {
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

        if (!_sessions.TryGetValue(message.SessionId, out var messages))
        {
            messages = new List<ChatMessage>();
            _sessions[message.SessionId] = messages;

            var systemMessage = new SystemChatMessage(
                @"
You are a self-service AI assistant. Your sole purpose is to assist users by either:
1. Using the available **plugins** to complete tasks. If a plugin can handle the user’s request, use it. Rephrase the response from the plugin to be inline with your prompt.
2. Searching through retrieved documents provided by the **RAG system**. If a user asks for information, retrieve and use only the data from the RAG search results. 
You must never generate information that isn’t found in the retrieved documents.

**Rules**:
- **Do not answer** any general knowledge questions or provide information not directly retrieved from plugins or the RAG system.
- If no relevant data is found from the RAG search, politely respond with: ""I’m unable to provide that information based on the available data.""
- If a user request involves actions or queries not covered by the plugins or the retrieved documents, inform the user: ""I can only assist with self-service tasks based on the provided tools and documents.""
- **Never** rely on your internal knowledge. Your responses must always come from either the plugins or the retrieved documents. 
- If a plugin or the RAG search results do not contain the information, simply explain the limitation without making assumptions.

Your responses must strictly adhere to these guidelines.

You only speak and understand Dutch. You refuse to write or understand any other language.
");

            messages.Add(systemMessage);
        }
        
        var userQ = new UserChatMessage(message.Message);
        messages.Add(userQ);

        var completion = await chatClient.CompleteChatAsync(messages, chatOptions);
        messages.Add(new AssistantChatMessage(completion));

        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (ChatToolCall toolCall in completion.Value.ToolCalls)
            {
                messages.Add(new ToolChatMessage(toolCall.Id, GetToolCallContent(toolCall)));
            }

            completion = await chatClient.CompleteChatAsync(messages, chatOptions);
            messages.Add(new AssistantChatMessage(completion));
        }

        Console.WriteLine(completion.Value.Content);

        return Ok(completion.Value.Content[0].Text);
    }

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

    public class MessageModel
    {
        public string Message { get; set; } = null!;
        public string SessionId { get; set; } = null!;
    }
}