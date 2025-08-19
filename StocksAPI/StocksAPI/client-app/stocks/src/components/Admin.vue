<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'

interface ProductItem {
  productId: string
  productName: string
  category: string
  colorCode: string
  stockCount: number
}

const products = ref<ProductItem[]>([])
const loading = ref(false)
const search = ref('')

// Define table headers
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
    title: 'Actions',
    key: 'actions',
    sortable: false,
    width: '100px'
  }
] as const;

const fetchProducts = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/products`)
    products.value = response.data.items as ProductItem[]
    console.log('Products fetched successfully:', response.data)
    console.log('Products fetched successfully:', products.value)
  } catch (error) {
    console.error('Error fetching products:', error)
  } finally {
    loading.value = false
  }
}

const getStockStatus = (stockCount: number) => {
  if (stockCount === 0) return { color: 'red', text: 'Out of Stock' }
  if (stockCount < 10) return { color: 'orange', text: 'Low Stock' }
  return { color: 'green', text: 'In Stock' }
}

const editProduct = (product: ProductItem) => {
  console.log('Edit product:', product)
  // Add edit functionality here
}

const deleteProduct = (product: ProductItem) => {
  console.log('Delete product:', product)
  // Add delete functionality here
}

onMounted(() => {
  fetchProducts()
})
</script>

<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="d-flex align-center pe-2">
            <v-icon icon="mdi-package-variant" class="me-3"></v-icon>
            <span class="text-h5">Product Management</span>
            <v-spacer></v-spacer>
            <v-btn 
              color="primary" 
              prepend-icon="mdi-plus"
              variant="elevated"
            >
              Add Product
            </v-btn>
          </v-card-title>
          
          <v-divider></v-divider>
          
          <v-card-text>
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
            
            <!-- Data table -->
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
              <!-- Product ID slot with truncated display -->
              <template v-slot:item.productId="{ item }">
                <v-tooltip :text="item.productId">
                  <template v-slot:activator="{ props }">
                    <span v-bind="props" class="text-truncate d-inline-block" style="max-width: 200px;">
                      {{ item.productId }}
                    </span>
                  </template>
                </v-tooltip>
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
              
              <!-- Actions slot -->
              <template v-slot:item.actions="{ item }">
                <v-btn
                  icon="mdi-pencil"
                  size="small"
                  variant="text"
                  color="primary"
                  @click="editProduct(item)"
                >
                </v-btn>
                <v-btn
                  icon="mdi-delete"
                  size="small"
                  variant="text"
                  color="error"
                  @click="deleteProduct(item)"
                >
                </v-btn>
              </template>
              
              <!-- Loading slot -->
              <template v-slot:loading>
                <v-skeleton-loader type="table-row@10"></v-skeleton-loader>
              </template>
            </v-data-table>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<style scoped>
.text-truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
