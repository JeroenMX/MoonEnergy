<template>
  <div class="chart-container">
    <div class="range-bar">
      <div
          class="actual-value"
          :style="{ left: actualPosition + '%' }"
          :class="{ low: isTooLow }"
      ></div>
      <div
          class="vertical-divider"
          :style="{ left: actualPosition + '%' }"
      ></div>
      <div
          class="current-label"
          :style="{ left: actualPosition + '%' }"
      >
        Huidig
      </div>
    </div>
    <div class="labels">
      <span class="min-label">{{ parsedData?.min }}</span>
      <span class="ideal-label">{{ parsedData?.ideal }}</span>
      <span class="max-label">{{ parsedData?.max }}</span>
    </div>
    <p class="feedback" :class="{'warning': isTooLow}">{{ feedback }}</p>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, watch, computed } from 'vue';

export default defineComponent({
  name: 'InstallmentChart',
  props: {
    data: {
      type: String,
      required: true
    }
  },
  setup(props) {
    const parsedData = ref<any>(null);
    const isTooLow = ref(false);
    const feedback = ref('');

    // Watch and parse the string to JSON
    watch(() => props.data, (newValue) => {
      try {
        parsedData.value = JSON.parse(newValue);
        isTooLow.value = parsedData.value?.actual < parsedData.value?.min;
        feedback.value = parsedData.value?.text || '';
      } catch (error) {
        console.error("Failed to parse JSON:", error);
      }
    }, { immediate: true });

    // Calculate the position of the actual value as a percentage
    const actualPosition = computed(() => {
      const min = parsedData.value?.min || 0;
      const max = parsedData.value?.max || 100;
      const actual = parsedData.value?.actual || 0;

      // If actual is less than min, place it at the leftmost
      if (actual < min) return 0;

      // If actual exceeds max, place it at the rightmost
      if (actual > max) return 100;

      // Calculate the percentage position of the actual value
      return ((actual - min) / (max - min)) * 100; // Returns a value between 0 and 100
    });

    return {
      parsedData,
      actualPosition,
      isTooLow,
      feedback
    };
  }
});
</script>

<style scoped>
.chart-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 300px;
  height: 200px;
  margin: auto;
}

.range-bar {
  position: relative;
  width: 100%;
  height: 30px; /* Adjust thickness */
  background: linear-gradient(to right,
  #FF3B3B 0%,   /* Red - Min */
  #08b532 50%,  /* Green - Ideal */
  #FF3B3B 100%  /* Red - Max */
  );
  border-radius: 5px;
  margin: 10px 0;
}

.actual-value {
  position: absolute;
  width: 10px; /* Marker thickness */
  height: 30px; /* Same as range-bar height */
  background-color: transparent; /* Make it transparent */
  top: 0;
  transform: translateX(-50%); /* Center the marker on the actual value */
}

.vertical-divider {
  position: absolute;
  width: 2px; /* Divider thickness */
  height: 60px; /* Longer than the range-bar height */
  background-color: blue; /* Change color to blue */
  top: -15px; /* Position above the bar */
  transform: translateX(-50%); /* Center the divider on the actual value */
}

.current-label {
  position: absolute;
  top: -35px; /* Adjust this value to move the label above the vertical line */
  left: 50%; /* Center horizontally */
  transform: translateX(-50%); /* Center the label */
  font-weight: bold; /* Optional: make the label bold */
}

.actual-value.low {
  background-color: #FF3B3B; /* Change color if too low */
}

.labels {
  display: flex;
  justify-content: space-between;
  width: 100%;
  font-size: 14px;
}

.feedback {
  margin-top: 10px;
  font-size: 14px;
  text-align: center;
}

.warning {
  color: #FF3B3B;
  font-weight: bold;
}
</style>
