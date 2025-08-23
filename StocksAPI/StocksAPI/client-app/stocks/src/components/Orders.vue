<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import type ProductItem from '../models/ProductItem'
import { ProductType, getProductTypeDisplayName } from '../models/ProductType'

interface OrderItem {
  productId: string
  colorId: string
  productName: string
  category: ProductType
  colorCode: string
  quantity: number
}

const products = ref<ProductItem[]>([])
const loading = ref(false)
const search = ref('')
const showProducts = ref(false)
const orderItems = ref<OrderItem[]>([])
const quantities = ref<Record<string, number>>({})

// Notification state
const snackbar = ref(false)
const snackbarText = ref('')
const snackbarColor = ref('success')

// Define table headers for products
const headers = [
  {
    title: 'Product Name',
    key: 'productName',
    sortable: true
  },
  {
    title: 'Category',
    key: 'category',
    sortable: true
  },
  {
    title: 'Color',
    key: 'colorCode',
    sortable: true,
    width: '120px'
  },
  {
    title: 'Stock Count',
    key: 'stockCount',
    sortable: true,
    align: 'end',
    width: '130px'
  },
  {
    title: 'Quantity',
    key: 'quantity',
    sortable: false,
    width: '120px'
  },
  {
    title: 'Actions',
    key: 'actions',
    sortable: false,
    width: '100px'
  }
] as const;

// Define table headers for order items
const orderHeaders = [
  {
    title: 'Product Name',
    key: 'productName',
    sortable: true
  },
  {
    title: 'Category',
    key: 'category',
    sortable: true
  },
  {
    title: 'Color',
    key: 'colorCode',
    sortable: true,
    width: '120px'
  },
  {
    title: 'Quantity',
    key: 'quantity',
    sortable: true,
    align: 'end',
    width: '100px'
  },
  {
    title: 'Actions',
    key: 'actions',
    sortable: false,
    width: '100px'
  }
] as const;

const totalItems = computed(() => {
  return orderItems.value.reduce((total, item) => total + item.quantity, 0)
})

const getItemKey = (item: ProductItem) => {
  return `${item.productId}-${item.colorId}`
}

const fetchProducts = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/products`)
    products.value = response.data.items as ProductItem[]
    console.log('Products fetched successfully:', response.data)
  } catch (error) {
    console.error('Error fetching products:', error)
    showNotification('Failed to load products', 'error')
  } finally {
    loading.value = false
  }
}

const getStockStatus = (stockCount: number) => {
  if (stockCount === 0) return { color: 'red', text: 'Out of Stock' }
  if (stockCount < 10) return { color: 'orange', text: 'Low Stock' }
  return { color: 'green', text: 'In Stock' }
}

const openOrder = async () => {
  showProducts.value = true
  await fetchProducts()
  loadOrderFromStorage()
}

const addToOrder = (product: ProductItem) => {
  const itemKey = getItemKey(product)
  const quantity = quantities.value[itemKey] || 0
  
  if (quantity <= 0) {
    showNotification('Please enter a valid quantity', 'error')
    return
  }

  if (quantity > product.stockCount) {
    showNotification('Quantity cannot exceed available stock', 'error')
    return
  }

  // Check if item already exists in order
  const existingItemIndex = orderItems.value.findIndex(item => 
    item.productId === product.productId && item.colorId === product.colorId
  )

  if (existingItemIndex >= 0) {
    // Update existing item
    orderItems.value[existingItemIndex].quantity += quantity
  } else {
    // Add new item
    orderItems.value.push({
      productId: product.productId,
      colorId: product.colorId,
      productName: product.productName,
      category: product.category,
      colorCode: product.colorCode,
      quantity: quantity
    })
  }

  // Clear the quantity input
  quantities.value[itemKey] = 0

  // Save to localStorage
  saveOrderToStorage()

  showNotification(`Added ${quantity} items to order`, 'success')
}

const removeFromOrder = (orderItem: OrderItem) => {
  const index = orderItems.value.findIndex(item => 
    item.productId === orderItem.productId && item.colorId === orderItem.colorId
  )
  
  if (index >= 0) {
    orderItems.value.splice(index, 1)
    saveOrderToStorage()
    showNotification('Item removed from order', 'success')
  }
}

const saveOrderToStorage = () => {
  localStorage.setItem('currentOrder', JSON.stringify(orderItems.value))
}

const loadOrderFromStorage = () => {
  const savedOrder = localStorage.getItem('currentOrder')
  if (savedOrder) {
    try {
      orderItems.value = JSON.parse(savedOrder)
    } catch (error) {
      console.error('Error loading order from storage:', error)
      orderItems.value = []
    }
  }
}

const clearOrder = () => {
  orderItems.value = []
  localStorage.removeItem('currentOrder')
  showNotification('Order cleared', 'success')
}

const showNotification = (message: string, color: 'success' | 'error') => {
  snackbarText.value = message
  snackbarColor.value = color
  snackbar.value = true
}

onMounted(() => {
  loadOrderFromStorage()
})
</script>

<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-clipboard-list" class="me-3"></v-icon>
            <span class="text-h5">Orders Management</span>
            <v-spacer></v-spacer>
            <v-btn 
              v-if="!showProducts"
              color="#021828" 
              prepend-icon="mdi-plus"
              variant="elevated"
              @click="openOrder"
            >
              Open Order
            </v-btn>
            <v-btn 
              v-if="showProducts && orderItems.length > 0"
              color="error" 
              prepend-icon="mdi-delete"
              variant="outlined"
              @click="clearOrder"
              class="me-2"
            >
              Clear Order
            </v-btn>
          </v-card-title>
          
          <v-divider></v-divider>
          
          <!-- Order Summary -->
          <v-card-text v-if="showProducts && orderItems.length > 0">
            <v-row>
              <v-col cols="12">
                <h3 class="text-h6 mb-4">
                  Current Order ({{ totalItems }} items)
                </h3>
                
                <v-data-table
                  :headers="orderHeaders"
                  :items="orderItems"
                  class="elevation-1 mb-6"
                  loading-text="Loading order items..."
                  no-data-text="No items in order"
                  items-per-page="5"
                  density="compact"
                >
                  <!-- Category slot with display name -->
                  <template v-slot:item.category="{ item }">
                    {{ getProductTypeDisplayName(item.category as ProductType) }}
                  </template>
                  
                  <!-- Color code slot with color chip -->
                  <template v-slot:item.colorCode="{ item }">
                    <v-chip
                      :color="item.colorCode"
                      size="small"
                      variant="elevated"
                      class="text-white"
                    >
                      {{ item.colorCode }}
                    </v-chip>
                  </template>
                  
                  <!-- Actions slot -->
                  <template v-slot:item.actions="{ item }">
                    <v-btn
                      icon="mdi-delete"
                      size="small"
                      variant="text"
                      color="error"
                      @click="removeFromOrder(item)"
                    >
                    </v-btn>
                  </template>
                </v-data-table>
              </v-col>
            </v-row>
          </v-card-text>

          <!-- Products Grid -->
          <v-card-text v-if="showProducts">
            <!-- Search field -->
            <v-row class="mb-4">
              <v-col cols="12" md="4">
                <v-text-field
                  v-model="search"
                  label="Search products..."
                  prepend-inner-icon="mdi-magnify"
                  variant="outlined"
                  clearable
                  density="compact"
                ></v-text-field>
              </v-col>
            </v-row>
            
            <!-- Products table -->
            <v-data-table
              v-model:search="search"
              :headers="headers"
              :items="products"
              :loading="loading"
              class="elevation-1"
              loading-text="Loading products..."
              no-data-text="No products found"
              items-per-page="10"
              show-current-page
            >
              <!-- Category slot with display name -->
              <template v-slot:item.category="{ item }">
                {{ getProductTypeDisplayName(item.category as ProductType) }}
              </template>
              
              <!-- Color code slot with color chip -->
              <template v-slot:item.colorCode="{ item }">
                <v-chip
                  :color="item.colorCode"
                  size="small"
                  variant="elevated"
                  class="text-white"
                >
                  {{ item.colorCode }}
                </v-chip>
              </template>
              
              <!-- Stock count slot with status indicator -->
              <template v-slot:item.stockCount="{ item }">
                <div class="d-flex align-center">
                  <span class="me-2">{{ item.stockCount }}</span>
                  <v-chip
                    :color="getStockStatus(item.stockCount).color"
                    size="x-small"
                    variant="elevated"
                  >
                    {{ getStockStatus(item.stockCount).text }}
                  </v-chip>
                </div>
              </template>

              <!-- Quantity input slot -->
              <template v-slot:item.quantity="{ item }">
                <v-number-input
                  v-model="quantities[getItemKey(item)]"
                  density="compact"
                  variant="outlined"
                  :min="0"
                  :max="item.stockCount"
                  step="1"
                  style="width: 100px;"
                ></v-number-input>
              </template>
              
              <!-- Actions slot -->
              <template v-slot:item.actions="{ item }">
                <v-btn
                  color="#021828"
                  size="small"
                  variant="elevated"
                  @click="addToOrder(item)"
                  :disabled="!quantities[getItemKey(item)] || quantities[getItemKey(item)] <= 0"
                >
                  Add
                </v-btn>
              </template>
              
              <!-- Loading slot -->
              <template v-slot:loading>
                <v-skeleton-loader type="table-row@10"></v-skeleton-loader>
              </template>
            </v-data-table>
          </v-card-text>

          <!-- Empty state when no order is open -->
          <v-card-text v-if="!showProducts" class="text-center py-12">
            <v-icon icon="mdi-clipboard-outline" size="64" color="grey-lighten-1" class="mb-4"></v-icon>
            <h3 class="text-h6 text-grey-darken-1 mb-2">No Order Open</h3>
            <p class="text-body-2 text-grey-darken-1">Click "Open Order" to start adding products to your order.</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <!-- Notification Snackbar -->
    <v-snackbar
      v-model="snackbar"
      :color="snackbarColor"
      timeout="4000"
      location="top right"
    >
      {{ snackbarText }}
      <template v-slot:actions>
        <v-btn
          variant="text"
          @click="snackbar = false"
        >
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<style scoped>
</style>
