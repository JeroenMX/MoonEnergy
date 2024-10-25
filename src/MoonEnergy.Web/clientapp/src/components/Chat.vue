<template>
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
      <input class="form-control" v-model="userInput" @keyup.enter="sendMessage" placeholder="Type your message..."
             autofocus>
      <button class="btn btn-primary" @click="sendMessage" :disabled="isLoading">Send</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import {onMounted, onUpdated, ref} from 'vue';
import {v4 as uuidv4} from 'uuid';
import {ChatInteraction} from "./Models.ts";
import LoginComponent from "./LoginComponent.vue";
import GetTermijnBedragComponent from "./GetTermijnBedragComponent.vue";
import GetEnergyConsumptionComponent from "./GetEnergyConsumptionComponent.vue";
import ErrorComponent from "./ErrorComponent.vue";
import {InitChat, SendChat} from "../services/ChatService.ts";
import {Interaction} from "chart.js";

const userInput = ref('');
const chat = ref<ChatInteraction[]>([]);
const isLoading = ref(false);
const messagesContainer = ref(null);
const sessionId = ref('');

const components = {
  LoginTool: LoginComponent,
  GetTermijnbedragTool: GetTermijnBedragComponent,
  GetEnergyConsumptionTool: GetEnergyConsumptionComponent,
  Error: ErrorComponent
};

onMounted(async () => {

  isLoading.value = true;
  sessionId.value = getSessionId();

  try {
    const interactions = await InitChat(sessionId.value);

    for (const interaction of interactions) {
      await handleInteraction(interaction);
    }
  } catch (error) {
    const errorMessage = createChatMessage('An error occurred while processing your request.', 'error');
    const errorInteraction = createInteraction(errorMessage);
    chat.value.push(errorInteraction);
  } finally {
    isLoading.value = false;
  }
});

const sendMessage = async () => {
  if (userInput.value.trim() === '' || isLoading.value) return;

  const message = userInput.value;
  const userMessage = createChatMessage(message, 'user');
  const userChatInteraction = createInteraction(userMessage);
  chat.value.push(userChatInteraction);

  userInput.value = '';
  isLoading.value = true;

  try {
    const interaction = await SendChat(sessionId.value, message);
    await handleInteraction(interaction);
  } catch (error) {
    console.error('Error:', error);
    const errorMessage = createChatMessage('An error occurred while processing your request.', 'error');
    userChatInteraction.messages.push(errorMessage);
  } finally {
    isLoading.value = false;
  }
};

function createInteraction(message: ChatMessage | null): ChatInteraction {
  const chatMessage: ChatMessage = {messages: [], actions: []};

  if (message != null) {
    chatMessage.messages.push(message);
  }

  return chatMessage;
}

function createChatMessage(message: string, type: string): ChatInteraction {
  return {type: type, text: message};
}

function getSessionId(): string {
  let sessionId: string = null;

  const sessionIdFromQueryString = getQueryStringParameter('sessionId');

  if (sessionIdFromQueryString == null) {
    sessionId = uuidv4();
  } else {

    const url = new URL(window.location.href);
    url.searchParams.delete('sessionId');
    window.history.replaceState({}, document.title, url.toString());

    sessionId = sessionIdFromQueryString;
  }

  return sessionId;
}

async function handleInteraction(interaction: ChatInteraction): Promise<void> {
  chat.value.push(interaction);

  interaction.actions.forEach(action => {
    // login
    if (action.action === 1) {
      handleLogin();
    }
  });
}

function handleLogin(): Promise<void> {
  setTimeout(() => {
        document.location.href = ` / bff / login ? returnUrl = /?sessionId=${sessionId.value}`;
      }

      ,
      5000
  )
  ;
}

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