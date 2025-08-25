import 'vuetify/styles'
import { createApp, watch } from 'vue'
import './style.css'
import '@mdi/font/css/materialdesignicons.css'

import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

import { aliases, mdi } from 'vuetify/iconsets/mdi'
import { createAuth0, useAuth0 } from '@auth0/auth0-vue';

import App from './App.vue'

const vuetify = createVuetify({
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: {
      mdi,
    },
  },
  components,
  directives,
})

import { createWebHistory, createRouter } from 'vue-router'

import OrdersView from './components/Orders.vue'
import AdminView from './components/Admin.vue'
import HelloView from './components/Hello.vue'

const routes = [
  { path: '/', component: HelloView },
  { path: '/orders', component: OrdersView },
  { path: '/admin', component: AdminView, meta: { requiresAdmin: true } },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})


router.beforeEach(async (to, from, next) => {
  const { user, isAuthenticated, isLoading } = useAuth0()

  if (to.meta.requiresAdmin) {
    // Wait for Auth0 to finish loading
    if (isLoading.value) {
      // Wait for auth to complete
      await new Promise((resolve) => {
        const unwatch = watch(isLoading, (loading) => {
          if (!loading) {
            unwatch()
            resolve(true)
          }
        })
      })
    }

    if (!isAuthenticated.value) {
      // Redirect to login
      console.log('User not authenticated');
      next('/')
      return
    }
    console.log('User in guard:', user.value);
    // Check admin role
    const roles = user.value['stocks/roles'] || [];

    if (!roles.includes('Admin')) {
      // Redirect to unauthorized or home
      next('/')
      return
    }
  }

  next()
})

const auth = createAuth0({
  domain: import.meta.env.VITE_AUTH0_DOMAIN,
  clientId: import.meta.env.VITE_AUTH0_CLIENT_ID,
  authorizationParams: {
    redirect_uri: window.location.origin,
    audience: import.meta.env.VITE_AUTH0_AUDIENCE
  },
  // Enable caching to persist authentication
  cacheLocation: 'localstorage', // Store tokens in localStorage instead of memory
  useRefreshTokens: true, // Enable refresh tokens
})

createApp(App).use(vuetify).use(router).use(auth).mount('#app')
