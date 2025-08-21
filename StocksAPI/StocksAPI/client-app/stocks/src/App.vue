<script setup lang="ts">

import { ref, watch, computed } from 'vue'
import { RouterLink, useRoute } from 'vue-router'

 const drawer = ref(false)
const group = ref(null)
  const route = useRoute()

// Computed property to check if we're on the home page
const isHomePage = computed(() => route.path === '/')

  watch(group, () => {
    drawer.value = false
  })
</script>

<template>
  <v-responsive class="border rounded">
  <v-app >
      <v-app-bar elevation="5" color="#021828">
        <v-app-bar-nav-icon variant="text" @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
       <v-app-bar-title>Stocks</v-app-bar-title>
        <v-spacer></v-spacer>
           <!-- Logo - only show when not on home page -->
        <img 
          v-if="!isHomePage"
          src="./assets/tth_logo_top2.png" 
          alt="TTH Logo" 
          class="app-bar-logo me-3"
        />
      </v-app-bar>

      <v-navigation-drawer
       v-model="drawer"
        :location="undefined"
        temporary>
       <v-list-item link title="Orders" to="/orders"  ></v-list-item>
       <v-list-item link title="Admin" to="/admin"></v-list-item>
      </v-navigation-drawer>

      <v-main style="background-color: #DCC0A1;">
        <v-container :fluid="true">
          <RouterView />
        </v-container>
      </v-main>
    </v-app>
  </v-responsive>
</template>

<style scoped>
.app-bar-logo {
  height: 40px;
  width: auto;
  object-fit: contain;
}
</style>
