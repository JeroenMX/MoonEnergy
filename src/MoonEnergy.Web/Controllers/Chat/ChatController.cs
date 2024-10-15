using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MoonEnergy.Controllers.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Controllers.Chat;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IEnumerable<IChatTool> _chatTools;

    //private static Dictionary<Guid, ChatHistory> _sessions;
    //private IFunctionHandler _functionHandler;
    private readonly OpenAiConfig _config;
    private static readonly Dictionary<string, ChatSession> _sessions = new();

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
    [Route("init")]
    public async Task<IActionResult> PostInit([FromBody] string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var chatSession))
        {
            chatSession = InitChatSession(sessionId);
            _sessions[chatSession.SessionId] = chatSession;
        }

        var chatClient = new ChatClient("gpt-4o", _config.Key);
        var allMessages = chatSession.Interactions.SelectMany(x => x.Messages);

        if (HandleUserLogin(chatSession))
        {
            var chatInteraction = new ChatInteraction { Actions = [], Messages = [] };
            chatSession.Interactions.Add(chatInteraction);

            chatInteraction.Messages.Add(new SystemChatMessage(
                $"Welcome user {chatSession.Name}, they have either just logged in or left and came back. Mention their first name in further conversations."));

            var completion = await chatClient.CompleteChatAsync(allMessages);
            chatInteraction.Messages.Add(new AssistantChatMessage(completion));

            var lastResponse = chatInteraction.Messages.Last().Content[0].Text;
            return Ok(new { Response = lastResponse, Actions = new List<ChatAction>() });
        }

        var chatInteraction2 = new ChatInteraction { Actions = [], Messages = [] };
        chatSession.Interactions.Add(chatInteraction2);

        chatInteraction2.Messages.Add(new SystemChatMessage(
            $"A visitor just started a session. Welcome them."));
        
        var completion2 = await chatClient.CompleteChatAsync(allMessages);
        chatInteraction2.Messages.Add(new AssistantChatMessage(completion2));
        
        var lastResponse2 = chatInteraction2.Messages.Last().Content[0].Text;
        return Ok(new { Response = lastResponse2, Actions = new List<ChatAction>() });
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MessageModel message)
    {
        var chatClient = new ChatClient("gpt-4o", _config.Key);
        var chatOptions = GetChatCompletionOptions();

        if (!_sessions.TryGetValue(message.SessionId, out var chatSession))
        {
            throw new Exception("No init called?");
        }

        var userQ = new UserChatMessage(message.Message);

        var chatInteraction = new ChatInteraction
        {
            Actions = [],
            Messages = [userQ]
        };

        chatSession.Interactions.Add(chatInteraction);
        var allMessages = chatSession.Interactions.SelectMany(x => x.Messages);

        var completion = await chatClient.CompleteChatAsync(allMessages, chatOptions);
        chatInteraction.Messages.Add(new AssistantChatMessage(completion));

        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in completion.Value.ToolCalls)
            {
                var toolContent = GetToolCallContent(toolCall);
                chatInteraction.Actions.Add(new ChatAction
                    { Action = toolContent.ActionType, ActionContentAsJson = toolContent.Json });
                chatInteraction.Messages.Add(new ToolChatMessage(toolCall.Id, toolContent.Text));
            }

            allMessages = chatSession.Interactions.SelectMany(x => x.Messages);
            completion = await chatClient.CompleteChatAsync(allMessages, chatOptions);
            chatInteraction.Messages.Add(new AssistantChatMessage(completion));
        }

        var lastResponse = chatInteraction.Messages.Last().Content[0].Text;
        var actions = chatInteraction.Actions;

        return Ok(new { Response = lastResponse, Actions = actions });
    }

    ChatToolResponse GetToolCallContent(ChatToolCall toolCall)
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

    private ChatCompletionOptions GetChatCompletionOptions()
    {
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

        return chatOptions;
    }

    private static ChatSession InitChatSession(string sessionId)
    {
        var chatSession = new ChatSession { SessionId = sessionId, Interactions = new() };
        _sessions[sessionId] = chatSession;

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

        // messages.Add(systemMessage);
        //
        // string filePath = "content.txt";
        //
        // // Read content from the file
        // string fileContent = await System.IO.File.ReadAllTextAsync(filePath);
        // var fileContentMessage = new SystemChatMessage(fileContent);
        // messages.Add(fileContentMessage);

        return chatSession;
    }

    private bool HandleUserLogin(ChatSession chatSession)
    {
        // user just logged in. send a welcome!
        if (HttpContext.User.Identity?.IsAuthenticated == true && chatSession.IsAuthenticated == false)
        {
            var userName = HttpContext.User.Claims.Single(x => x.Type == "name").Value;
            chatSession.IsAuthenticated = true;
            chatSession.Name = userName;

            return true;
        }

        return false;
    }
}