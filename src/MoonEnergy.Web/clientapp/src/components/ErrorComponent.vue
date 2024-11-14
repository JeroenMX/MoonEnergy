<template>
  <div class="alert alert-danger d-flex align-items-center" role="alert">
    <font-awesome-icon :icon="['fas', 'exclamation-circle']" class="fa-2x me-2" />
    <div>{{ parsedData?.message }}</div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, watch, computed } from 'vue';
import {FontAwesomeIcon} from '@fortawesome/vue-fontawesome'
import { library } from '@fortawesome/fontawesome-svg-core'
import {faExclamationCircle} from '@fortawesome/free-solid-svg-icons'

export default defineComponent({
  name: 'ErrorComponent',
  components: {FontAwesomeIcon},
  props: {
    data: {
      type: String,
      required: true
    }
  },
  setup(props) {
    library.add(faExclamationCircle);
    const parsedData = ref<any>(null);

    // Watch and parse the string to JSON
    watch(() => props.data, (newValue) => {
      try {
        parsedData.value = JSON.parse(newValue);
      } catch (error) {
        console.error("Failed to parse JSON:", error);
      }
    }, { immediate: true });
    
    return {
      parsedData
    };
  }
});
</script>

<style scoped>

</style>
