<script setup lang="ts">
import Chat from './components/Chat.vue'
import {getUserName} from "./services/AuthenticationService.ts";
import {onMounted, ref} from "vue";

const userNameLabel = ref('');

onMounted(async () => {
  const userName = await getUserName();

  if (userName !== null) {
    userNameLabel.value = userName;
  } else {
    userNameLabel.value = "not logged in"
  }
});

</script>

<template>
  <div id="currentUser">{{ userNameLabel }}</div>
  <Chat/>
</template>

<style scoped>

</style>
