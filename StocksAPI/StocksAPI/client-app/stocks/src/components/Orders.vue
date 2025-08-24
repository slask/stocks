<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import axios from 'axios'
import type ProductItem from '../models/ProductItem'
import { ProductType, getProductTypeDisplayName } from '../models/ProductType'
import type OrderItem from '../models/OrderItem'

const products = ref<ProductItem[]>([])
const loading = ref(false)
const search = ref('')
const showProducts = ref(false)
const orderItems = ref<OrderItem[]>([])
const quantities = ref<Record<string, number>>({})
const drawerOpen = ref(true)
const clearOrderDialog = ref(false)
const finalizeOrderDialog = ref(false)
const finalizingOrder = ref(false)
const clientName = ref('')
const clientNameDialog = ref(false)
const savingClientName = ref(false)

// Client name validation rules
const clientNameRules = [
  (v: string) => !!v || 'Client name is required',
  (v: string) => v.length <= 100 || 'Client name must be less than 100 characters'
]

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

const totalUniqueProducts = computed(() => {
  const uniqueProducts = new Set()
  orderItems.value.forEach(item => {
    uniqueProducts.add(item.productId + ';' + item.colorId)
  })
  return uniqueProducts.size
})

const getItemKey = (item: ProductItem) => {
  return `${item.productId}-${item.colorId}`
}

const fetchProducts = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/products`)
    products.value = response.data.items as ProductItem[]
    
  } catch (error) {
    console.error('Error fetching products:', error)
    showNotification('Failed to load products', 'error')
  } finally {
    loading.value = false
  }
}

const getStockStatus = (stockCount: number) => {
  if (stockCount === 0) return { color: '#EB0E0E', text: 'Out of Stock' }
  if (stockCount < 10) return { color: 'orange', text: 'Low Stock' }
  return { color: '#21603D', text: 'In Stock' }
}


const openOrder = async () => {
  clientNameDialog.value = true
}

const confirmClientName = async () => {
  if (!clientName.value || clientName.value.length > 100) {
    showNotification('Please enter a valid client name (max 100 characters)', 'error')
    return
  }
  
  savingClientName.value = true
  clientNameDialog.value = false
  showProducts.value = true
  
  await fetchProducts()
  loadOrderFromStorage()
  saveOrderToStorage() // Save client name immediately
  
  savingClientName.value = false
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
  const orderData = {
    clientName: clientName.value,
    items: orderItems.value
  }
  localStorage.setItem('currentOrder', JSON.stringify(orderData))
}

const loadOrderFromStorage = () => {
  const savedOrder = localStorage.getItem('currentOrder')
  if (savedOrder) {
    try {
      const orderData = JSON.parse(savedOrder)

      orderItems.value = orderData.items
      clientName.value = orderData.clientName || ''

    } catch (error) {
      console.error('Error loading order from storage:', error)
      orderItems.value = []
      clientName.value = ''
    }
  }
}

const openClearOrderDialog = () => {
  clearOrderDialog.value = true
}

const confirmClearOrder = () => {
  orderItems.value = []
  clientName.value = ''
  localStorage.removeItem('currentOrder')
  clearOrderDialog.value = false
  showNotification('Order cleared', 'success')
}

const closeClearOrderDialog = () => {
  clearOrderDialog.value = false
}

const showNotification = (message: string, color: 'success' | 'error') => {
  snackbarText.value = message
  snackbarColor.value = color ==='error'? '#EB0E0E' : '#21603D'
  snackbar.value = true
}

const openFinalizeOrderDialog = () => {
  finalizeOrderDialog.value = true
}

const confirmFinalizeOrder = async () => {
  finalizingOrder.value = true
  
  try {
 
    const response = await axios.post(`${import.meta.env.VITE_API_URL}/api/orders`, {
      clientName: clientName.value,
      items: orderItems.value
    })
    
    // For now, just clear the order and show success
    orderItems.value = []
    clientName.value = ''
    localStorage.removeItem('currentOrder')
    finalizeOrderDialog.value = false
    showProducts.value = false
    
    showNotification('Order finalized successfully! Stock quantities have been updated.', 'success')
  } catch (error: any) {
    const errorMessage = error.response?.data?.message || error.message || 'Failed to finalize order'
    showNotification(`Error: ${errorMessage}`, 'error')
    console.error('Error finalizing order:', error)
  } finally {
    finalizingOrder.value = false
  }
}

const closeFinalizeOrderDialog = () => {
  finalizeOrderDialog.value = false
}

// Custom search function for multi-word search
const customSearch = (value: any, query: string, item?: any) => {
  
  if (query == null || query === '') return true
  
  const searchTerms = query.toLowerCase().split(' ').filter(term => term.trim() !== '')
  const searchText = item?.columns.productName.toLowerCase() + ' ' + item?.columns.category.toLowerCase() + ' ' + item?.columns.colorCode?.toLowerCase() || value.toString().toLowerCase() || '';

  // Return true if every search term matches
  return searchTerms.every(term => searchText.includes(term))
}

onMounted(() => {
  loadOrderFromStorage()
  
  // If there's an existing order, show the products view
  if (clientName.value) {
    showProducts.value = true
    fetchProducts()
  }
})
</script>

<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-clipboard-list" class="me-3"></v-icon>
            <div class="flex-grow-1">
    <span class="text-h5">Orders Management</span>
    <div v-if="showProducts && clientName" class="text-caption text-grey-darken-1">
      Client: {{ clientName }}
    </div>
  </div>
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
          </v-card-title>
          
          <v-divider></v-divider>
          
         <!-- Order Summary Toggle Button -->
<v-card-text v-if="showProducts && orderItems.length > 0" class="py-2">
  <v-row>
    <v-col cols="12" class="d-flex justify-end">
      <v-btn
        :color="drawerOpen ? 'grey' : '#021828'"
        :variant="drawerOpen ? 'outlined' : 'elevated'"
        :prepend-icon="drawerOpen ? 'mdi-chevron-right' : 'mdi-chevron-left'"
        @click="drawerOpen = !drawerOpen"
      >
        {{ drawerOpen ? 'Hide' : 'Show' }} Order ({{ totalItems }} items, {{ totalUniqueProducts }} products)
      </v-btn>
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
              :custom-filter="customSearch"
              :filter-keys="['productName', 'category', 'colorCode']"
              class="elevation-2"
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
                  color="#C6C6C6"
                  size="small"
                  variant="elevated"
                  class="text-black"
                >
                  {{ item.colorCode }}
                </v-chip>
              </template>
              
              <!-- Stock count slot with status indicator -->
          <template v-slot:item.stockCount="{ item }">
  <div class="d-flex align-center">
    <v-chip
      :color="getStockStatus(item.stockCount).color"
      size="x-small"
      :text="item.stockCount"
      variant="elevated"
    >
    </v-chip>
  </div>
</template>

              <!-- Quantity input slot -->
              <template v-slot:item.quantity="{ item }">
                <v-number-input
                  v-model="quantities[getItemKey(item)]"
                  density="compact"
                  variant="outlined"
                  :min=0
                  :max="item.stockCount"
                  :step=1
                  style="width: 170px;"
                ></v-number-input>
              </template>
              
              <!-- Actions slot -->
              <template v-slot:item.actions="{ item }">
                <v-btn
                  color="#1CB8E8"
                  size="small"
                  class="text-white"
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

    <!-- Client Name Dialog -->
<v-dialog v-model="clientNameDialog" max-width="500px" persistent>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-icon icon="mdi-account" class="me-3"></v-icon>
      <span class="text-h5">Client Information</span>
    </v-card-title>
    
    <v-divider></v-divider>
    
    <v-card-text class="pt-6">
      <v-form>
        <v-text-field
          v-model="clientName"
          label="Client Name"
          placeholder="Enter client name"
          variant="outlined"
          :rules="clientNameRules"
          counter="100"
          maxlength="100"
          required
          autofocus
        ></v-text-field>
      </v-form>
    </v-card-text>
    
    <v-card-actions class="px-6 pb-6">
      <v-spacer></v-spacer>
      <v-btn
        color="#021828"
        variant="elevated"
        @click="confirmClientName"
        :loading="savingClientName"
        :disabled="savingClientName || !clientName"
      >
        Start Order
      </v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>

<!-- Clear Order Confirmation Dialog -->
<v-dialog v-model="clearOrderDialog" max-width="500px" persistent>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-icon icon="mdi-alert" class="me-3" color="error"></v-icon>
      <span class="text-h5">Clear Order</span>
    </v-card-title>
    
    <v-divider></v-divider>
    
    <v-card-text class="pt-6">
      <div class="text-body-1 mb-4">
        Are you sure you want to clear the entire order for <strong>{{ clientName }}</strong>?
      </div>
      
      <div class="text-body-2 text-grey-darken-1 mb-4">
        This will remove all {{ totalItems }} items ({{ totalUniqueProducts }} products) from the order.
      </div>
      
      <v-alert 
        type="warning" 
        variant="tonal"
        class="mb-0"
      >
        <strong>Warning:</strong> This action cannot be undone.
      </v-alert>
    </v-card-text>
    
    <v-card-actions class="px-6 pb-6">
      <v-spacer></v-spacer>
      <v-btn
        variant="outlined"
        @click="closeClearOrderDialog"
      >
        Cancel
      </v-btn>
      <v-btn
        color="error"
        variant="elevated"
        @click="confirmClearOrder"
      >
        Clear Order
      </v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>

<!-- Finalize Order Confirmation Dialog -->
<v-dialog v-model="finalizeOrderDialog" max-width="600px" persistent>
  <v-card>
    <v-card-title class="d-flex align-center">
      <v-icon icon="mdi-check-circle" class="me-3" color="success"></v-icon>
      <span class="text-h5">Finalize Order</span>
    </v-card-title>
    
    <v-divider></v-divider>
    
    <v-card-text class="pt-6">
      <div class="text-body-1 mb-4">
        <strong>Please review your order carefully before finalizing:</strong>
      </div>
      
      <v-card variant="outlined" class="mb-4">
        <v-card-text>
          <div class="text-subtitle-1 mb-2"><strong>Client:</strong> {{ clientName }}</div>
          <div class="text-subtitle-1 mb-2"><strong>Total Items:</strong> {{ totalItems }}</div>
          <div class="text-subtitle-1"><strong>Unique Products:</strong> {{ totalUniqueProducts }}</div>
        </v-card-text>
      </v-card>
      
      <v-alert 
        type="warning" 
        variant="tonal"
        class="mb-4"
      >
        <strong>Important:</strong> After submitting this order, the quantities will be automatically deducted from stock and this action cannot be undone.
      </v-alert>
      
      <div class="text-body-2 text-grey-darken-1">
        Are you sure you want to finalize this order?
      </div>
    </v-card-text>
    
    <v-card-actions class="px-6 pb-6">
      <v-spacer></v-spacer>
      <v-btn
        variant="outlined"
        @click="closeFinalizeOrderDialog"
        :disabled="finalizingOrder"
      >
        Cancel
      </v-btn>
      <v-btn
        color="#021828"
        variant="elevated"
        class="text-white"
        @click="confirmFinalizeOrder"
        :loading="finalizingOrder"
        :disabled="finalizingOrder"
      >
        Finalize Order
      </v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>

    <!-- Order Items Drawer -->
    <v-navigation-drawer
      v-model="drawerOpen"
      location="right"
      temporary
      width="700"
      v-if="showProducts && orderItems.length > 0"
    >
      <v-card flat>
<v-card-title class="d-flex align-center">
  <v-icon icon="mdi-clipboard-list" class="me-3"></v-icon>
  <div class="flex-grow-1">
    <span class="text-h6">Current Order</span>
    <div class="text-caption text-grey-darken-1">Client: {{ clientName }}</div>
  </div>
  <div class="d-flex gap-2 me-3">
    <v-chip color="#021828" variant="elevated" class="text-white">
      {{ totalItems }} items
    </v-chip>
    <v-chip color="#DCC0A1" variant="elevated" class="text-black">
      {{ totalUniqueProducts }} products
    </v-chip>
  </div>
  <v-btn
    icon="mdi-chevron-right"
    variant="text"
    size="small"
    @click="drawerOpen = false"
  >
  </v-btn>
</v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text class="pa-0">
          <v-data-table
            :headers="orderHeaders"
            :items="orderItems"
            loading-text="Loading order items..."
            no-data-text="No items in order"
            items-per-page="10"
            density="compact"
            hide-default-footer
          >
            <!-- Category slot with display name -->
            <template v-slot:item.category="{ item }">
              {{ getProductTypeDisplayName(item.category as ProductType) }}
            </template>
            
            <!-- Color code slot with color chip -->
            <template v-slot:item.colorCode="{ item }">
              <v-chip
                color="#C6C6C6"
                size="small"
                variant="elevated"
                class="text-black"
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
        </v-card-text>
        
        <v-divider></v-divider>
        
        <v-card-actions class="justify-center">
  <v-btn
    color="error"
    prepend-icon="mdi-delete"
    variant="outlined"
    @click="openClearOrderDialog"
    :disabled="orderItems.length === 0"
  >
    Clear Order
  </v-btn>
  <v-btn
    color="#021828"
    prepend-icon="mdi-check"
    variant="elevated"
    class="text-white me-2"
    @click="openFinalizeOrderDialog"
    :disabled="orderItems.length === 0"
  >
    Finalize Order
  </v-btn>
</v-card-actions>
      </v-card>
    </v-navigation-drawer>

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