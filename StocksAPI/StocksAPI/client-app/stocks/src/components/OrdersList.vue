<template>
  <div>
    <v-card-text>
      <v-data-table-server
        v-model:page="currentPage"
        :headers="ordersHeaders"
        :items="orders"
        :loading="loadingOrders"
        :items-length="totalOrders"
        :items-per-page="itemsPerPage"
        @update:page="onPageChange"
        @update:items-per-page="onItemsPerPageChange"
        class="elevation-2"
        loading-text="Loading orders..."
        no-data-text="No orders found"
        show-current-page
        :items-per-page-options="[
          { value: 10, title: '10' },
          { value: 25, title: '25' },
          { value: 50, title: '50' },
          { value: 100, title: '100' },
        ]"
      >
        <!-- Order Date slot with formatted date -->
        <template v-slot:item.orderDate="{ item }">
          {{ formatDate(item.orderDate) }}
        </template>

        <!-- Created By slot -->
        <template v-slot:item.createdBy="{ item }">
          {{ item.createdBy || 'Unknown' }}
        </template>

        <!-- Loading slot -->
        <template v-slot:loading>
          <v-skeleton-loader type="table-row@10"></v-skeleton-loader>
        </template>
      </v-data-table-server>
    </v-card-text>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { useApi } from '../api'
  import type Order from '../models/Order'

  // State variables
  const orders = ref<Order[]>([])
  const loadingOrders = ref(false)
  const totalOrders = ref(0)
  const currentPage = ref(1)
  const itemsPerPage = ref(10)

  const { apiGet } = useApi()

  // Define emit for notifications
  const emit = defineEmits<{
    notification: [message: string, color: 'success' | 'error']
  }>()

  // Table headers
  const ordersHeaders = [
    {
      title: 'Client Name',
      key: 'clientName',
      sortable: false,
    },
    {
      title: 'Order Date',
      key: 'orderDate',
      sortable: false,
    },
    {
      title: 'Created By',
      key: 'createdBy',
      sortable: false,
    },
  ] as const

  // Fetch orders from API
  const fetchOrders = async (page = 1, pageSize = itemsPerPage.value) => {
    loadingOrders.value = true
    try {
      const response = await apiGet(`/api/orders?pageNumber=${page}&pageSize=${pageSize}`)
      const data = response.data

      orders.value = data.items.map((order: any) => ({
        orderId: order.orderId,
        clientName: order.clientName,
        orderDate: new Date(order.orderDate),
        createdBy: order.createdBy || 'Unknown',
      }))

      totalOrders.value = data.totalCount
      currentPage.value = data.pageNumber
    } catch (error: any) {
      console.error('Error fetching orders:', error)
      if (error.response?.status === 403) {
        emit('notification', 'You do not have permission to view orders.', 'error')
        return
      }
      emit('notification', 'Failed to load orders', 'error')
    } finally {
      loadingOrders.value = false
    }
  }

  // Handle page changes
  const onPageChange = (page: number) => {
    currentPage.value = page
    fetchOrders(page, itemsPerPage.value)
  }

  // Handle items per page changes
  const onItemsPerPageChange = (newItemsPerPage: number) => {
    itemsPerPage.value = newItemsPerPage
    currentPage.value = 1 // Reset to first page when changing items per page
    fetchOrders(1, newItemsPerPage)
  }

  // Format date for display
  const formatDate = (date: Date) => {
    return date.toLocaleDateString()
  }

  // Expose refresh method for parent component
  const refreshOrders = () => {
    fetchOrders(currentPage.value, itemsPerPage.value)
  }

  // Expose the refresh method
  defineExpose({
    refreshOrders,
  })

  // Load orders on mount
  onMounted(() => {
    fetchOrders()
  })
</script>
