<script setup lang="ts">

</script>

<template>
  <div class="logo-container">
    <img 
      src="../assets/tth_splash.png" 
      alt="TTH Logo" 
      class="logo-image"
    />
  </div>
</template>

<style scoped>
.logo-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  width: 100%;
}

.logo-image {
  max-width: 100%;
  max-height: 80vh;
  object-fit: contain;
}
</style>
