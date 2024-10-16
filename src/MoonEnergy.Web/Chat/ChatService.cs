using System.Security.Claims;
using Microsoft.Extensions.Options;
using MoonEnergy.Chat.Base;
using OpenAI.Chat;

namespace MoonEnergy.Chat;

public class ChatService
{
    private readonly IEnumerable<IChatTool> _chatTools;

    //private static Dictionary<Guid, ChatHistory> _sessions;
    //private IFunctionHandler _functionHandler;
    private readonly OpenAiConfig _config;
    private static readonly Dictionary<string, ChatSession> _sessions = new();

    public ChatService(IOptions<OpenAiConfig> config, IEnumerable<IChatTool> chatTools)
    {
        _chatTools = chatTools;
        _config = config.Value;
    }

    public async Task<ChatSession> Init(string sessionId, ClaimsPrincipal user)
    {
        if (!_sessions.TryGetValue(sessionId, out var chatSession))
        {
            chatSession = InitChatSession(sessionId);
            _sessions[chatSession.SessionId] = chatSession;
        }

        var chatClient = new ChatClient("gpt-4o", _config.Key);
        var chatOptions = GetChatCompletionOptions();

        // the user just authenticated or an authenticated user returned.
        if (HandleUserLogin(chatSession, user) || chatSession.IsAuthenticated)
        {
            var chatInteraction = new ChatInteraction { Actions = [], Messages = [] };
            chatSession.Interactions.Add(chatInteraction);

            chatInteraction.Messages.Add(new SystemChatMessage(
                @$"
Welcome user {chatSession.Name}, they have either just logged in or came back after they left. 
Mention their first name in further conversations. They cannot change their name anymore under no circumstance.
They are now logged in. The conversation resumes and any previous question can now be answered and tools needed to be called can do so.
"));
            
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
        }
        else
        {
            var chatInteraction = new ChatInteraction { Actions = [], Messages = [] };
            chatSession.Interactions.Add(chatInteraction);

            chatInteraction.Messages.Add(new SystemChatMessage(
                @$"
An anonymous user just started a session. They are not logged in. Welcome them. Ask them their name unless they are going to login anyway. 
Mention their first name in further conversations. When a tool requires a logged in user call the login tool.
"));

            var allMessages = chatSession.Interactions.SelectMany(x => x.Messages);
            var completion = await chatClient.CompleteChatAsync(allMessages);
            chatInteraction.Messages.Add(new AssistantChatMessage(completion));
        }

        return chatSession;
    }

    public async Task<ChatInteraction> Interact(string sessionId, string message)
    {
        var chatClient = new ChatClient("gpt-4o", _config.Key);
        var chatOptions = GetChatCompletionOptions();

        if (!_sessions.TryGetValue(sessionId, out var chatSession))
        {
            throw new Exception("No init called?");
        }

        var userQ = new UserChatMessage(message);

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

        return chatInteraction;
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

    private bool HandleUserLogin(ChatSession chatSession, ClaimsPrincipal user)
    {
        // user just logged in. send a welcome!
        if (user.Identity?.IsAuthenticated == true && chatSession.IsAuthenticated == false)
        {
            var userName = user.Claims.Single(x => x.Type == "name").Value;
            chatSession.IsAuthenticated = true;
            chatSession.Name = userName;

            return true;
        }

        return false;
    }
}