<template>
  <div>
    <select v-model="selectedDeviceId" @change="selectDevice" :disabled="isRecording">
      <option v-for="device in audioDevices" :key="device.deviceId" :value="device.deviceId">
        {{ device.label || 'Unknown Device' }}
      </option>
    </select>
    <button @click="startRecording" :disabled="isRecording">Start Recording</button>
    <button @click="stopRecording" :disabled="!isRecording">Stop Recording</button>
    <button @click="playRecording" :disabled="!recordedAudio">Play Recording</button>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from 'vue';
import SignalRService from '@/services/SignalRService';

export default defineComponent({
  name: 'AudioRecorder',
  setup() {
    const isRecording = ref(false);
    const recordedAudio = ref<Blob | null>(null);
    const audioDevices = ref<MediaDeviceInfo[]>([]);
    const selectedDeviceId = ref<string | null>(null);
    let mediaRecorder: MediaRecorder;
    let audioChunks: BlobPart[] = [];
    let audioContext: AudioContext;
    let audioBuffer: AudioBuffer;

    onMounted(async () => {
      try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        audioDevices.value = devices.filter(device => device.kind === 'audioinput');

        // Connect to SignalR hub when the component is mounted
        await SignalRService.connect();
      } catch (error) {
        console.error('Error during component mount:', error);
      }
    });

    const selectDevice = () => {
      stopRecording();
    };

    const startRecording = async () => {
      try {
        if (!selectedDeviceId.value) {
          throw new Error('No audio input device selected');
        }

        audioChunks = [];
        const stream = await navigator.mediaDevices.getUserMedia({
          audio: { deviceId: { exact: selectedDeviceId.value } },
        });

        audioContext = new AudioContext();
        
        const options = { audioBitsPerSecond: 24000 };
        mediaRecorder = new MediaRecorder(stream, options);

        mediaRecorder.ondataavailable = (event) => {
          if (event.data.size > 0) {
            audioChunks.push(event.data);
          }
        };

        mediaRecorder.onstop = async () => {
          const audioBlob = new Blob(audioChunks, { type: 'audio/wav' }); // audio/wav
          recordedAudio.value = audioBlob;

          const arrayBuffer = await audioBlob.arrayBuffer();
          audioBuffer = await audioContext.decodeAudioData(arrayBuffer);

          // Convert the audio buffer to mono Int16 array
          const monoData = audioBuffer.getChannelData(0);
          const int16Array = new Int16Array(monoData.length);

          // Convert the audio buffer to pcm16.
          for (let i = 0; i < monoData.length; i++) {
            int16Array[i] = monoData[i] * 0x7FFF;
          }

          console.log(int16Array);

          const base64EncodedAudio = base64EncodeFromArrayBuffer(int16Array.buffer);

          // Stream the audio data to SignalR hub
          //SignalRService.sendAudioData(int16Array);
          SignalRService.SendAudioDataBase64(base64EncodedAudio);
        };

        mediaRecorder.start();
        isRecording.value = true;
      } catch (error) {
        console.error('Error starting recording:', error);
      }
    };

    const base64EncodeFromArrayBuffer = (arrayBuffer: ArrayBuffer): string => {
      let binary = '';
      const bytes = new Uint8Array(arrayBuffer);
      const len = bytes.byteLength;
      for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
      }
      return btoa(binary);
    };

    const stopRecording = () => {
      if (mediaRecorder && mediaRecorder.state === 'recording') {
        mediaRecorder.stop();
        isRecording.value = false;
      }
    };

    const playRecording = () => {
      if (recordedAudio.value) {
        const audioUrl = URL.createObjectURL(recordedAudio.value);
        const audio = new Audio(audioUrl);
        audio.play();
      }
    };

    return {
      audioDevices, selectedDeviceId, selectDevice, startRecording, stopRecording, playRecording, isRecording, recordedAudio
    };
  }
});
</script>

<style scoped>
select {
  padding: 10px;
  margin: 5px;
  font-size: 16px;
}

button {
  padding: 10px;
  margin: 5px;
  font-size: 16px;
}
</style>