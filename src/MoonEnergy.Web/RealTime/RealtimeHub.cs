using Microsoft.AspNetCore.SignalR;

namespace MoonEnergy.RealTime;

public class RealtimeHub : Hub
{
    private readonly RealtimeService _realtimeService;

    public RealtimeHub(RealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

   
    public async Task StreamAudio(byte[] audioData)
    {
        await _realtimeService.SendBinaryDataAsync(audioData);

        // Assuming you have logic to process the audio data, for example, saving it or sending it to all connected clients
        //await Clients.All.SendAsync("ReceiveAudio", audioData);
    }
    
    public async Task SendAudioData(byte[] audioData)
    {
        await _realtimeService.SendBinaryDataAsync(audioData);

        // Assuming you have logic to process the audio data, for example, saving it or sending it to all connected clients
        //await Clients.All.SendAsync("ReceiveAudio", audioData);
    }
    
    public async Task SendAudioDataBase64(string audioData)
    {
        await _realtimeService.SendBinaryDataAsync(audioData);
    }
}