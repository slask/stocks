<script setup lang="ts">

import { ref, watch } from 'vue'


 const drawer = ref(false)
  const group = ref(null)

  watch(group, () => {
    drawer.value = false
  })
</script>

<template>
  <v-responsive class="border rounded">
  <v-app >
      <v-app-bar elevation="5" color="primary">
        <v-app-bar-nav-icon variant="text" @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
       <v-app-bar-title>TTH Stocks</v-app-bar-title>
      </v-app-bar>

      <v-navigation-drawer
       v-model="drawer"
        :location="undefined"
        temporary>
       <v-list-item link title="Orders" to="/orders"></v-list-item>
       <v-list-item link title="Admin" to="/admin"></v-list-item>
      </v-navigation-drawer>

      <v-main style="background-color: #f5f5d1;">
        <v-container :fluid="true">
          <RouterView />
        </v-container>
      </v-main>
    </v-app>
  </v-responsive>
</template>

<style scoped>

</style>
