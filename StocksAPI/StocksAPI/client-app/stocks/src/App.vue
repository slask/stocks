<script setup lang="ts">
  import { ref, watch, computed } from 'vue'
  import { useRoute } from 'vue-router'
  import { useAuth0 } from '@auth0/auth0-vue'

  const drawer = ref(false)
  const group = ref(null)
  const route = useRoute()

  // User menu dropdown
  const userMenu = ref(false)

  // Auth0 composable
  const { loginWithRedirect, logout, user, isAuthenticated, isLoading } = useAuth0()

  // Computed property to check if we're on the home page
  const isHomePage = computed(() => route.path === '/')

  watch(group, () => {
    drawer.value = false
  })

  const handleLogin = () => {
    loginWithRedirect()
  }

  const handleLogout = () => {
    logout({
      logoutParams: {
        returnTo: window.location.origin,
      },
    })
  }

  // Computed property to check if user has admin role
  const isAdmin = computed(() => {
    if (!user.value) return false

    // Check for roles in different possible locations
    const roles = user.value['stocks/roles'] || []

    return Array.isArray(roles) ? roles.includes('Admin') : false
  })
</script>

<template>
  <v-responsive class="border rounded">
    <v-app>
      <v-app-bar elevation="5" color="#021828">
        <v-app-bar-nav-icon variant="text" @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
        <v-app-bar-title>
          <router-link to="/" class="title-link">Stocks</router-link>
        </v-app-bar-title>
        <v-spacer></v-spacer>

        <!-- Auth Section -->
        <div v-if="!isLoading" class="d-flex align-center">
          <!-- Login Button (when not authenticated) -->
          <v-btn
            v-if="!isAuthenticated"
            color="white"
            variant="outlined"
            @click="handleLogin"
            prepend-icon="mdi-login"
            class="mr-3"
          >
            Login
          </v-btn>

          <!-- User Menu (when authenticated) -->
          <v-menu v-if="isAuthenticated" v-model="userMenu" offset-y>
            <template v-slot:activator="{ props }">
              <v-btn v-bind="props" variant="text" class="text-white mr-3" append-icon="mdi-chevron-down">
                <v-avatar size="32" class="me-2">
                  <v-img v-if="user?.picture" :src="user.picture" :alt="user.name"></v-img>
                  <v-icon v-else icon="mdi-account-circle"></v-icon>
                </v-avatar>
                {{ user?.username || user?.name || user?.email }}
              </v-btn>
            </template>

            <v-list>
              <v-list-item>
                <v-list-item-title class="font-weight-bold">
                  {{ user?.username || user?.name }}
                </v-list-item-title>
                <v-list-item-subtitle>
                  {{ user?.email }}
                </v-list-item-subtitle>
              </v-list-item>

              <v-divider></v-divider>

              <v-list-item @click="handleLogout">
                <template v-slot:prepend>
                  <v-icon icon="mdi-logout"></v-icon>
                </template>
                <v-list-item-title>Logout</v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
        </div>

        <!-- Loading indicator -->
        <v-progress-circular v-if="isLoading" indeterminate size="24" color="white"></v-progress-circular>

        <!-- Logo - only show when not on home page -->
        <img v-if="!isHomePage" src="./assets/tth_logo_top2.png" alt="TTH Logo" class="app-bar-logo me-3" />
      </v-app-bar>

      <v-navigation-drawer v-model="drawer" :location="undefined" temporary>
        <v-list-item link title="Orders" to="/orders"></v-list-item>
        <v-list-item v-if="isAdmin" link title="Admin" to="/admin"></v-list-item>
      </v-navigation-drawer>

      <v-main style="background-color: #dcc0a1">
        <v-container :fluid="true">
          <div v-if="!isLoading">
            <RouterView v-if="isAuthenticated" />
            <div v-else class="text-center pa-12">
              <v-icon icon="mdi-lock" size="64" color="red" class="mb-4"></v-icon>
              <h2 class="text-h4 mb-4">Authentication Required</h2>
              <p class="text-body-1 mb-6">Please log in to access the Teletehnica Stocks application.</p>
              <v-btn color="#021828" variant="elevated" size="large" @click="handleLogin" prepend-icon="mdi-login">
                Login to Continue
              </v-btn>
            </div>
          </div>
          <div v-else class="text-center pa-12">
            <v-progress-circular indeterminate size="64" color="#021828"></v-progress-circular>
            <p class="text-body-1 mt-4">Loading...</p>
          </div>
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

  .title-link {
    text-decoration: none;
    color: inherit;
    cursor: pointer;
  }

  .title-link:hover {
    opacity: 0.8;
  }
</style>
