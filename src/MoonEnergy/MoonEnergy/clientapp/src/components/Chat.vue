<template>
  <div class="chat-container">
    <div class="messages" ref="messagesContainer">
      <div v-for="(message, index) in messages" :key="index" :class="message.type">
        {{ message.text }}
      </div>
    </div>
    <div class="input-area">
      <input v-model="userInput" @keyup.enter="sendMessage" placeholder="Type your message...">
      <button @click="sendMessage" :disabled="isLoading">Send</button>
    </div>
  </div>
</template>

<script setup>
import { ref, onUpdated } from 'vue';
import { v4 as uuidv4 } from 'uuid';

const userInput = ref('');
const messages = ref([]);
const isLoading = ref(false);
const messagesContainer = ref(null);
const sessionId = uuidv4();

const sendMessage = async () => {
  if (userInput.value.trim() === '' || isLoading.value) return;

  const message = userInput.value;
  messages.value.push({ type: 'user', text: message });
  userInput.value = '';
  isLoading.value = true;

  const payload = {
    message,
    sessionId
  };

  try {
    const response = await fetch('/api/chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      throw new Error('API request failed');
    }

    const data = await response.text();
    messages.value.push({ type: 'api', text: data });
  } catch (error) {
    console.error('Error:', error);
    messages.value.push({ type: 'error', text: 'An error occurred while processing your request.' });
  } finally {
    isLoading.value = false;
  }
};

onUpdated(() => {
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
});
</script>

<style scoped>
.chat-container {
  max-width: 500px;
  margin: 0 auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 5px;
}

.messages {
  height: 300px;
  overflow-y: auto;
  margin-bottom: 20px;
  padding: 10px;
  border: 1px solid #eee;
  border-radius: 5px;
}

.user {
  text-align: right;
  color: blue;
}

.api {
  text-align: left;
  color: green;
}

.error {
  text-align: left;
  color: red;
}

.input-area {
  display: flex;
}

input {
  flex-grow: 1;
  padding: 5px;
  margin-right: 10px;
}

button {
  padding: 5px 10px;
}
</style>