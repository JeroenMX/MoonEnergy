<template>
  <font-awesome-icon :icon="['fas', 'sign-in-alt']"/>
</template>

<script setup lang="ts">
import {FontAwesomeIcon} from '@fortawesome/vue-fontawesome'
import {library} from '@fortawesome/fontawesome-svg-core'
import {faSignInAlt} from '@fortawesome/free-solid-svg-icons'
import {getUserName} from "../services/AuthenticationService.ts";
import {onMounted} from "vue";

interface Props {
  sessionId: string | null;
}

const props = defineProps<{
  data: string // The JSON string that needs to be parsed
}>();

// Parse the data prop to the Props type
const parsedData: Props = JSON.parse(props.data);

library.add(faSignInAlt);

onMounted(async () => {
  const userName = await getUserName();

  if (userName == null) {
    handleLogin();
  }
  
});

function handleLogin(): void {
  setTimeout(() => {
    document.location.href = `/bff/login?returnUrl=/?sessionId=${parsedData.sessionId}`;
  }, 5000);
}

</script>

<style scoped>

</style>