import 'vuetify/styles'
import { createApp } from 'vue'
import './style.css'
import '@mdi/font/css/materialdesignicons.css'

import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

import { aliases, mdi } from 'vuetify/iconsets/mdi'

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
  { path: '/admin', component: AdminView },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

createApp(App).use(vuetify).use(router).mount('#app')
