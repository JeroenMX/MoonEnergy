import * as signalR from '@microsoft/signalr';
import {MessagePackHubProtocol} from '@microsoft/signalr-protocol-msgpack';

class SignalRService {
    private connection: signalR.HubConnection;
    private audioContext: AudioContext;
    private audioSource: MediaStreamAudioSourceNode;
    private processor: ScriptProcessorNode;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/realtimehub', {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets,
            })
            .withHubProtocol(new MessagePackHubProtocol()) // Use MessagePack
            .configureLogging(signalR.LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        // Add handlers for connection close and error
        this.connection.onclose((error) => {
            console.error('SignalR connection closed:', error);
        });

        this.connection.onreconnecting((error) => {
            console.warn('SignalR connection lost. Reconnecting...', error);
        });

        this.connection.on('ReceiveAudioFragment', this.handleIncomingAudioFragment);
    }

    public async connect(): Promise<void> {
        try {
            await this.connection.start();
            console.log('SignalR connection established.');
        } catch (err) {
            console.error('Error establishing SignalR connection:', err.toString());
        }
    }

    public async startStreamingAudio(stream: MediaStream): Promise<void> {
        this.audioContext = new AudioContext();
        this.audioSource = this.audioContext.createMediaStreamSource(stream);
        this.processor = this.audioContext.createScriptProcessor(1024, 1, 1);

        this.processor.onaudioprocess = (event) => {
            const audioData = event.inputBuffer.getChannelData(0);
            const audioDataArray = Int16Array.from(audioData.map(n => n * 0x7FFF)); // Convert to 16-bit PCM
            const audioDataBytes = new Uint8Array(audioDataArray.buffer);
            this.sendAudioDataFragment(audioDataBytes);
        };

        this.audioSource.connect(this.processor);
        this.processor.connect(this.audioContext.destination);
    }

    private async sendAudioDataFragment(audioDataFragment: Uint8Array): Promise<void> {
        try {
            await this.connection.invoke('StreamAudio', audioDataFragment);
        } catch (err) {
            console.error('Error sending audio data fragment:', err.toString());
        }
    }

    // ArrayBuffer or Int16Array
    public SendAudioDataBase64(audioData: string): void {
        try {
            this.connection.invoke('SendAudioData', audioData);
        } catch (err) {
            console.error('Error sending audio data fragment:', err.toString());
        }
    }

    private handleIncomingAudioFragment(audioDataFragment: Uint8Array): void {
        // Handle incoming audio fragments for playback
        // This part will need a mechanism to buffer and play audio in real-time
    }
}

export default new SignalRService();