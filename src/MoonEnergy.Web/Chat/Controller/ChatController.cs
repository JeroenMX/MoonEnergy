﻿using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace MoonEnergy.Chat.Controller;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
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
        var chatSession = await _chatService.Init(sessionId, HttpContext.User);
        var messages = chatSession.Interactions.Select(Convert).ToList();

        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MessageModel message)
    {
        var response = await _chatService.Interact(message.SessionId, message.Message);
        var model = Convert(response);

        return Ok(model);
    }

    private static ChatInteraction Convert(Base.ChatInteraction chatInteraction)
    {
        return new ChatInteraction
        {
            Actions = chatInteraction.Actions.Select(a => new ChatAction
            {
                Action = (int)a.Action,
                ContentAsJson = a.ActionContentAsJson
            }).ToList(),
            Messages = chatInteraction.Messages
                .Where(m => m is UserChatMessage or AssistantChatMessage)
                .Select(m => new ChatMessage
                {
                    Type = m is UserChatMessage ? "user" : "api",
                    Text = m.Content.Count > 0 ? m.Content[0].Text : string.Empty
                }).ToList()
        };
    }
}