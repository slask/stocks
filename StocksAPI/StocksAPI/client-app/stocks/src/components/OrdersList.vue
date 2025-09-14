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

        <!-- Actions slot -->
        <template v-slot:item.actions="{ item }">
          <v-btn
            icon="mdi-eye"
            size="small"
            variant="text"
            color="primary"
            @click="openOrderDetails(item)"
            :loading="loadingOrderDetails && selectedOrderId === item.orderId"
          ></v-btn>
        </template>

        <!-- Loading slot -->
        <template v-slot:loading>
          <v-skeleton-loader type="table-row@10"></v-skeleton-loader>
        </template>
      </v-data-table-server>
    </v-card-text>

    <!-- Order Details Drawer -->
    <v-navigation-drawer v-model="orderDetailsDrawer" location="right" temporary width="800">
      <v-card flat class="d-flex flex-column" style="height: 100vh">
        <v-card-title class="d-flex align-center flex-shrink-0">
          <v-icon icon="mdi-clipboard-text" class="me-3"></v-icon>
          <div class="flex-grow-1">
            <span class="text-h6">Order Details</span>
            <div v-if="selectedOrder" class="text-caption text-grey-darken-1">
              Client: {{ selectedOrder.clientName }} | Date: {{ formatDate(selectedOrder.orderDate) }}
            </div>
          </div>
          <v-btn icon="mdi-close" variant="text" size="small" @click="closeOrderDetails"></v-btn>
        </v-card-title>

        <v-divider class="flex-shrink-0"></v-divider>

        <v-card-text class="pa-0 flex-grow-1 d-flex flex-column" style="overflow: hidden">
          <v-data-table
            :headers="orderItemsHeaders"
            :items="orderItems"
            :loading="loadingOrderDetails"
            loading-text="Loading order items..."
            no-data-text="No items in this order"
            :items-per-page="10"
            density="compact"
            class="flex-grow-1"
            style="height: 100%"
            fixed-header
          >
            <!-- Category slot -->
            <template v-slot:item.category="{ item }">
              {{ item.category }}
            </template>

            <!-- Color code slot -->
            <template v-slot:item.colorCode="{ item }">
              <v-chip color="#C6C6C6" size="small" variant="elevated" class="text-black">
                {{ item.colorCode }}
              </v-chip>
            </template>

            <!-- Quantity slot -->
            <template v-slot:item.quantity="{ item }">
              <v-chip color="#021828" size="small" variant="elevated" class="text-white">
                {{ item.quantity }}
              </v-chip>
            </template>
          </v-data-table>
        </v-card-text>

        <v-divider class="flex-shrink-0"></v-divider>

        <v-card-actions class="justify-center flex-shrink-0">
          <v-btn color="grey-darken-1" prepend-icon="mdi-close" variant="outlined" @click="closeOrderDetails">
            Close
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-navigation-drawer>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { useApi } from '../api'
  import type { Order, OrderRow } from '../models/Order'

  // State variables
  const orders = ref<Order[]>([])
  const loadingOrders = ref(false)
  const totalOrders = ref(0)
  const currentPage = ref(1)
  const itemsPerPage = ref(10)

  // Order details drawer state
  const orderDetailsDrawer = ref(false)
  const selectedOrder = ref<Order | null>(null)
  const selectedOrderId = ref<string | null>(null)
  const orderItems = ref<OrderRow[]>([])
  const loadingOrderDetails = ref(false)

  const { apiGet } = useApi()

  // Define emit for notifications
  const emit = defineEmits<{
    notification: [message: string, color: 'success' | 'error']
  }>()

  // Table headers for orders list
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
    {
      title: 'Actions',
      key: 'actions',
      sortable: false,
      width: '100px',
    },
  ] as const

  // Table headers for order items in drawer
  const orderItemsHeaders = [
    {
      title: 'Product Name',
      key: 'productName',
      sortable: false,
    },
    {
      title: 'Category',
      key: 'category',
      sortable: false,
    },
    {
      title: 'Color',
      key: 'colorCode',
      sortable: false,
      width: '120px',
    },
    {
      title: 'Quantity',
      key: 'quantity',
      sortable: false,
      width: '100px',
      align: 'center',
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

  // Fetch order details and items
  const fetchOrderDetails = async (orderId: string) => {
    loadingOrderDetails.value = true
    try {
      const response = await apiGet(`/api/orders/${orderId}`)
      const data = response.data

      orderItems.value = data.items.map((item: any) => ({
        productId: item.productId,
        productName: item.productName,
        category: item.category,
        colorCode: item.colorCode,
        quantity: item.quantity,
      }))
    } catch (error: any) {
      console.error('Error fetching order details:', error)
      if (error.response?.status === 403) {
        emit('notification', 'You do not have permission to view order details.', 'error')
        return
      }
      emit('notification', 'Failed to load order details', 'error')
    } finally {
      loadingOrderDetails.value = false
    }
  }

  // Open order details drawer
  const openOrderDetails = async (order: Order) => {
    selectedOrder.value = order
    selectedOrderId.value = order.orderId
    orderDetailsDrawer.value = true
    await fetchOrderDetails(order.orderId)
  }

  // Close order details drawer
  const closeOrderDetails = () => {
    orderDetailsDrawer.value = false
    selectedOrder.value = null
    selectedOrderId.value = null
    orderItems.value = []
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
