<template>
  <div>
    <canvas ref="barChart"></canvas>
  </div>
</template>

<script lang="ts">
import { defineComponent, onMounted, ref, PropType } from 'vue';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

interface BarChartData {
  usage: number[];
}

export default defineComponent({
  name: 'BarChart',
  props: {
    data: {
      type: String as PropType<string>,
      required: true,
    },
  },
  setup(props) {
    const barChart = ref<HTMLCanvasElement | null>(null);

    onMounted(() => {
      const ctx = barChart.value?.getContext('2d');
      console.log('Canvas context:', ctx);

      if (ctx) {
        // Parse the JSON string
        let usageData: number[] = [];

        try {
          const parsedData: BarChartData = JSON.parse(props.data);
          usageData = Array.isArray(parsedData.usage) ? parsedData.usage : [];
        } catch (error) {
          console.error('Failed to parse JSON data:', error);
        }

        const labels = usageData.map((_, index) => `${index + 1}`);

        new Chart(ctx, {
          type: 'bar',
          data: {
            labels,
            datasets: [
              {
                label: 'Verbruik',
                data: usageData,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1,
              },
            ],
          },
          options: {
            animation: {
              duration: 2000, // duration of the animation in milliseconds
              easing: 'easeInOutQuad', // easing function
              onComplete: function() {
                console.log('Animation complete!');
              }
            },
            scales: {
              y: {
                beginAtZero: true,
              },
            },
          },
        });
      } else {
        console.error('Failed to get canvas context.');
      }
    });

    return {
      barChart,
    };
  },
});
</script>

<style scoped>
canvas {
  max-width: 100%;
  margin: auto;
  display: block; /* Center it horizontally in its container */
  width: 100%; /* This scales the element, but not the drawing surface */
}
</style>
