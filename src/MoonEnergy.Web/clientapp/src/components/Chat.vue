<template>
  <div id="currentUser">{{ userName }}</div>
  <div class="chat-container">
    <div class="messages" ref="messagesContainer">
      <div v-for="(interaction, chatIndex) in chat" :key="chatIndex">
        <div v-for="(message, messageIndex) in interaction.messages" :key="index" :class="message.type">
          {{ message.text }}
        </div>
        <div v-for="(action, actionIndex) in interaction.actions" :key="index">
          <component :is="components[action.name]" :data="action.contentAsJson"/>
        </div>
      </div>
    </div>
    <div class="input-area">
      <input class="form-control" v-model="userInput" @keyup.enter="sendMessage" placeholder="Type your message..." autofocus>
      <button class="btn btn-primary" @click="sendMessage" :disabled="isLoading">Send</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import {ref, onUpdated, onMounted} from 'vue';
import {v4 as uuidv4} from 'uuid';
import axios, {AxiosResponse} from "axios";
import {ChatInteraction} from "./Models.ts";
import LoginComponent from "./LoginComponent.vue";
import GetTermijnBedragComponent from "./GetTermijnBedragComponent.vue";
import GetEnergyConsumptionComponent from "./GetEnergyConsumptionComponent.vue";

const userInput = ref('');
const chat = ref<ChatInteraction[]>([]);
const isLoading = ref(false);
const messagesContainer = ref(null);
const sessionId = ref('');
const userName = ref()

const components = {
  LoginTool: LoginComponent,
  GetTermijnbedragTool: GetTermijnBedragComponent,
  GetEnergyConsumptionTool: GetEnergyConsumptionComponent
};

const getUser = async () => {
  const config = {
    headers: {
      'X-CSRF': '1'
    }
  }

  return await axios.get('/bff/user', {
    ...config,
    validateStatus: function (status) {
      return true;  // Resolve promise for all HTTP status codes
    }
  });
}

onMounted(async () => {

  const sessionIdFromQueryString = getQueryStringParameter('sessionId');

  if (sessionIdFromQueryString == null) {
    sessionId.value = uuidv4();
  } else {

    const url = new URL(window.location.href);
    url.searchParams.delete('sessionId');
    window.history.replaceState({}, document.title, url.toString());

    sessionId.value = sessionIdFromQueryString;
  }

  const user = await getUser();

  if (user.status === 200) {
    userName.value = user.data.find(x => x.type === 'name').value;
  } else {
    userName.value = "not logged in"
  }

  const response = await fetch('/api/chat/init', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(sessionId.value),
  });

  chat.value = await response.json();
});

const sendMessage = async () => {
  if (userInput.value.trim() === '' || isLoading.value) return;

  const message = userInput.value;

  const messageText = userInput.value;
  const userMessage: ChatMessage = {type: 'user', text: messageText};
  const userChatInteraction: ChatInteraction = {messages: [userMessage], actions: []};
  chat.value.push(userChatInteraction);

  userInput.value = '';
  isLoading.value = true;

  const payload = {
    message,
    sessionId: sessionId.value
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

    const chatInteraction = await response.json();
    chat.value.push(chatInteraction);

    chatInteraction.actions.forEach(action => {
      // login
      if (action.action === 1) {
        setTimeout(() => {
          document.location.href = `/bff/login?returnUrl=/?sessionId=${sessionId.value}`;
        }, 5000);
      }
    });

  } catch (error) {
    console.error('Error:', error);
    messages.value.push({type: 'error', text: 'An error occurred while processing your request.'});
  } finally {
    isLoading.value = false;
  }
};

function delay(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

// Helper function to get query string parameters
function getQueryStringParameter(param: string): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get(param);
}

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
  height: 600px;
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