using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Websocket.Client;

namespace MoonEnergy.RealTime;

public class RealtimeService
{
    private WebsocketClient? _client;
    private readonly OpenAiConfig _config;

    public RealtimeService(IOptions<OpenAiConfig> config)
    {
        _config = config.Value;
    }

    public async Task StartConnectionAsync()
    {
        var url = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-realtime-preview-2024-10-01");
        var factory = new Func<ClientWebSocket>(() =>
        {
            var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", $"Bearer {_config.Key}");
            socket.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");

            return socket;
        });

        _client = new WebsocketClient(url, factory);
        _client.MessageReceived.Subscribe(async msg => await HandleMessageAsync(msg));
        await _client.Start();

        await _client.SendInstant(UpdateSession());
        await _client.SendInstant(CreateTextMessage("Hello!"));
        await _client.SendInstant(CreateResponse());
    }

    public async Task SendBinaryDataAsync(byte[] data)
    {
        var base64AudioData = ConvertInt16ArrayToBase64(data);
        var itemCreateEvent = AudioToItemCreateEvent(base64AudioData);

        if (_client?.IsRunning == true)
        {
            await _client.SendInstant(itemCreateEvent);
            await _client.SendInstant(CreateResponse());
        }
    }

    public async Task SendBinaryDataAsync(string base64AudioData)
    {
        var itemCreateEvent = AudioToItemCreateEvent(base64AudioData);

        if (_client?.IsRunning == true)
        {
            await _client.SendInstant(itemCreateEvent);
            await _client.SendInstant(CreateResponse());
        }
    }

    private Task HandleMessageAsync(ResponseMessage msg)
    {
        if (msg.MessageType == WebSocketMessageType.Text)
        {
            Console.WriteLine(msg);
        }

        if (msg.MessageType == WebSocketMessageType.Binary)
        {
            Console.WriteLine(msg);
        }

        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(string message)
    {
        if (_client?.IsRunning == true)
        {
            await _client.SendInstant(message);
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_client != null)
        {
            await _client.Stop(WebSocketCloseStatus.NormalClosure, "Closing connection");
        }
    }

    private static string ConvertInt16ArrayToBase64(byte[] data)
    {
        // Convert byte array to Base64 encoded string
        return Convert.ToBase64String(data);
    }

    private string AudioToItemCreateEvent(string base64AudioData)
    {
        var eventObject = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new
                    {
                        type = "input_audio",
                        audio = base64AudioData
                    }
                }
            }
        };

        return JsonSerializer.Serialize(eventObject);
    }

    private string CreateTextMessage(string message)
    {
        var eventObject = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new
                    {
                        type = "input_text",
                        text = message
                    }
                }
            }
        };

        return JsonSerializer.Serialize(eventObject);
    }

    private string CreateResponse()
    {
        var resultObject = new
        {
            type = "response.create",
            response = new
            {
                modalities = new[] { "text" },
                instructions = "Describe that you heard and respond to the user."
            },
        };

        return JsonSerializer.Serialize(resultObject);
    }

    private string UpdateSession()
    {
        var obj = new
        {
            type = "session.update",
            session = new
            {
                turn_detection = (string?)null,
                input_audio_transcription = new
                {
                    model = "whisper-1"
                }
            }
        };

        return JsonSerializer.Serialize(obj);
    }
}