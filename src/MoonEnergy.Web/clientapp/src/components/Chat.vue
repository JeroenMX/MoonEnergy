<template>
  <div id="currentUser">{{ userName }}</div>
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

<script setup lang="ts">
import {ref, onUpdated, onMounted} from 'vue';
import {v4 as uuidv4} from 'uuid';
import axios, {AxiosResponse} from "axios";

const userInput = ref('');
const messages = ref([]);
const isLoading = ref(false);
const messagesContainer = ref(null);


const sessionId = ref('');

const userName = ref()

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
  }
  else
  {
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

  const chatInteraction = await response.json();
  messages.value.push({type: 'api', text: chatInteraction.response});
});


const sendMessage = async () => {
  if (userInput.value.trim() === '' || isLoading.value) return;

  const message = userInput.value;
  messages.value.push({type: 'user', text: message});
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
    messages.value.push({type: 'api', text: chatInteraction.response});

    chatInteraction.actions.forEach(action => {
      // login
      if (action.action === 1) {
        document.location.href = `/bff/login?returnUrl=/?sessionId=${sessionId.value}`;
      }
    });
    
  } catch (error) {
    console.error('Error:', error);
    messages.value.push({type: 'error', text: 'An error occurred while processing your request.'});
  } finally {
    isLoading.value = false;
  }
};

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