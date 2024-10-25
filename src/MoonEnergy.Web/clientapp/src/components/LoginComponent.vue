<script setup lang="ts">
import {FontAwesomeIcon} from '@fortawesome/vue-fontawesome'
import { library } from '@fortawesome/fontawesome-svg-core'
import {faSignInAlt} from '@fortawesome/free-solid-svg-icons'

library.add(faSignInAlt);

</script>

<template>
  <font-awesome-icon :icon="['fas', 'sign-in-alt']" />
</template>

<style scoped>

</style>