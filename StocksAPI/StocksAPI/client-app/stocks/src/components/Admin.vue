<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'
import type ProductItem from '../models/ProductItem'
import { ProductType, getProductTypeDisplayName } from '../models/ProductType'

const products = ref<ProductItem[]>([])
const loading = ref(false)
const search = ref('')
const dialog = ref(false)
const colorDialog = ref(false)
const saving = ref(false)
const savingColor = ref(false)

const deleteDialog = ref(false)
const deleting = ref(false)
const itemToDelete = ref<ProductItem | null>(null)

const editDialog = ref(false)
const savingEdit = ref(false)

// Form data
const newProduct = ref({
  productName: '',
  category: ProductType.KnittingThreads as ProductType,
})

// Color form data
const newColor = ref({
  productId: '',
  colorCode: '',
  stockCount: 0
})

const editProduct = ref({
  id: '',
  name: '',
  category: ProductType.KnittingThreads as ProductType,
})

// Form validation rules
const nameRules = [
  (v: string) => !!v || 'Product name is required',
  (v: string) => v.length >= 2 || 'Product name must be at least 2 characters',
  (v: string) => v.length <= 100 || 'Product name must be less than 100 characters'
]

const colorRules = [
  (v: string) => !!v || 'Color code is required'
]

const stockRules = [
  (v: number) => v >= 0 || 'Stock count must be 0 or greater'
]

const productSelectRules = [
  (v: string) => !!v || 'Please select a product'
]

// Notification state
const snackbar = ref(false)
const snackbarText = ref('')
const snackbarColor = ref('success')

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
    width: '100px'
  },
  {
    title: 'Actions',
    key: 'actions',
    sortable: false,
    width: '150px'
  }
] as const;

// Get ProductType values for dropdown
const productTypes = Object.values(ProductType).map(type => ({
  value: type,
  title: getProductTypeDisplayName(type)
}))

// Get unique products for color dropdown
const uniqueProducts = ref<Array<{value: string, title: string}>>([])

const updateUniqueProducts = () => {

  const productMap = new Map()
  products.value.forEach(product => {
    if (!productMap.has(product.productName)) {
      productMap.set(product.productName, {
        value: product.productId,
        title: product.productName
      })
    }
  })
  uniqueProducts.value = Array.from(productMap.values())
}

const fetchProducts = async () => {
  loading.value = true
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_URL}/api/products`)
    products.value = response.data.items as ProductItem[]
    updateUniqueProducts()
  } catch (error) {
    console.error('Error fetching products:', error)
  } finally {
    loading.value = false
  }
}

const getStockStatus = (stockCount: number) => {
  if (stockCount === 0) return { color: '#EB0E0E', text: 'Out of Stock' }
  if (stockCount < 10) return { color: 'orange', text: 'Low Stock' }
  return { color: '#21603D', text: 'In Stock' }
}

const openAddDialog = () => {
  // Reset form
  newProduct.value = {
    productName: '',
    category: ProductType.KnittingThreads,
  }
  dialog.value = true
}

const openAddColorDialog = () => {
  // Reset form
  newColor.value = {
    productId: '',
    colorCode: '',
    stockCount: 0
  }
  colorDialog.value = true
}

const closeDialog = () => {
  dialog.value = false
}

const closeColorDialog = () => {
  colorDialog.value = false
}

const saveProduct = async () => {
  // Validate form
  if (newProduct.value.productName.length < 2 || newProduct.value.productName.length > 100) {
    showNotification('Product name must be between 2 and 100 characters', 'error')
    return
  }

  saving.value = true
  try {
    const response = await axios.post(`${import.meta.env.VITE_API_URL}/api/product`, {
      name: newProduct.value.productName,
      category: newProduct.value.category,
    })
    console.log('Save Product response:', response.data)
    showNotification('Product saved successfully!', 'success')
    closeDialog()
    
    // Refresh the list
    await fetchProducts()
  } catch (error: any) {
    const errorMessage = error.response?.data?.errors.generalErrors[0] || error.response?.data?.message || error.message || 'Failed to save product'
    showNotification(`Error: ${errorMessage}`, 'error')
    console.error('Error saving product:', error)
  } finally {
    saving.value = false
  }
}

const saveColor = async () => {
  // Validate form
  if (!newColor.value.productId) {
    showNotification('Please select a product', 'error')
    return
  }

  if (!newColor.value.colorCode) {
    showNotification('Color code is required', 'error')
    return
  }

  if (newColor.value.stockCount < 0) {
    showNotification('Stock count must be 0 or greater', 'error')
    return
  }

  savingColor.value = true
  try {
    const response = await axios.post(`${import.meta.env.VITE_API_URL}/api/product/${newColor.value.productId}/colors`, 
      {
        code: newColor.value.colorCode,
        existingQuantity: newColor.value.stockCount
      }
    )
    console.log('Save Color variant response:', response.data)
    showNotification('Color variant saved successfully!', 'success')
    
    // Clear only color and stock fields, keep product selected
    newColor.value = {
      productId: newColor.value.productId, // Keep the selected product
      colorCode: '',
      stockCount: 0
    }
    
    // Refresh the list
    await fetchProducts()
  } catch (error: any) {
    const errorMessage = error.response?.data?.errors.generalErrors[0] || error.response?.data?.message || error.message || 'Failed to save color variant'
    showNotification(`Error: ${errorMessage}`, 'error')
    console.error('Error saving color variant:', error)
  } finally {
    savingColor.value = false
  }
}

const showNotification = (message: string, color: 'success' | 'error') => {
  snackbarText.value = message
  snackbarColor.value = color ==='error'? '#EB0E0E' : '#21603D'
  snackbar.value = true
}

const openEditDialog = (product: ProductItem) => {
  // Set form data from selected product
  editProduct.value = {
    id: product.productId,
    name: product.productName,
    category: product.category as ProductType,
  }
  editDialog.value = true
}

const closeEditDialog = () => {
  editDialog.value = false
}

const updateProduct = async () => {
  // Validate form
  if (editProduct.value.name.length < 2 || editProduct.value.name.length > 100) {
    showNotification('Product name must be between 2 and 100 characters', 'error')
    return
  }

  savingEdit.value = true
  try {
    const response = await axios.put(`${import.meta.env.VITE_API_URL}/api/product/${editProduct.value.id}`, {
      name: editProduct.value.name,
      category: editProduct.value.category,
    })
    console.log('Update response:', response)
    showNotification('Product updated successfully!', 'success')
    closeEditDialog()
    
    // Refresh the list
    await fetchProducts()
  } catch (error: any) {
    const errorMessage = error.response?.data?.errors.name[0] ||error.response?.data?.message || error.message || 'Failed to update product'
    showNotification(`Error: ${errorMessage}`, 'error')
    console.error('Error updating product:', error)
  } finally {
    savingEdit.value = false
  }
}

const deleteProduct = (product: ProductItem) => {
  openDeleteDialog(product)
}

const openDeleteDialog = (product: ProductItem) => {
  itemToDelete.value = product
  deleteDialog.value = true
}

const closeDeleteDialog = () => {
  deleteDialog.value = false
  itemToDelete.value = null
}

const confirmDelete = async () => {
  if (!itemToDelete.value) return

  const product = itemToDelete.value
  const hasColor = itemHasColor(product);

  deleting.value = true
  try {
    let response;
    if (hasColor) {
      // Delete specific color variant
      response = await axios.delete(`${import.meta.env.VITE_API_URL}/api/product/${product.productId}/colors/${product.colorId}`)
      showNotification('Color variant deleted successfully!', 'success')
    } else {
      // Delete entire product
      response = await axios.delete(`${import.meta.env.VITE_API_URL}/api/product/${product.productId}`)
      showNotification('Product deleted successfully!', 'success')
    }
    console.log('Delete response:', response.data)
    closeDeleteDialog()
    
    // Refresh the list
    await fetchProducts()
  } catch (error: any) {
    const errorMessage = error.response?.data?.errors?.generalErrors?.[0] || 
                        error.response?.data?.message || 
                        error.message || 
                        'Failed to delete item'
    showNotification(`Error: ${errorMessage}`, 'error')
    console.error('Error deleting item:', error)
  } finally {
    deleting.value = false
  }
}

const getDeleteMessage = () => {
  if (!itemToDelete.value) return ''
  
  const product = itemToDelete.value
  const hasColor = itemHasColor(product);

  if (hasColor) {
    return `Are you sure you want to delete the color variant "${product.colorCode}" from product "${product.productName}"?`
  } else {
    return `Are you sure you want to delete the entire product "${product.productName}"?`
  }
}

const getDeleteTitle = () => {
  if (!itemToDelete.value) return 'Delete Item'

  const hasColor = itemHasColor(itemToDelete.value)
  return hasColor ? 'Delete Color Variant' : 'Delete Product'
}

const itemHasColor = (product: ProductItem) => {
  return product.colorCode && product.colorCode.trim() !== '' && product.colorId;
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
              color="#021828" 
              prepend-icon="mdi-plus"
              variant="elevated"
              @click="openAddDialog"
              class="me-2"
            >
              Add Product
            </v-btn>
            <v-btn 
              color="#DCC0A1" 
              prepend-icon="mdi-palette"
              variant="elevated"
              @click="openAddColorDialog"
            >
              Add Color
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
              
              <!-- Actions slot -->
              <template v-slot:item.actions="{ item }">
                <v-btn
                  icon="mdi-pencil"
                  size="small"
                  variant="text"
                  color="#1CB8E8"
                  @click="openEditDialog(item)"
                >
                </v-btn>
                <v-btn
                  icon="mdi-delete"
                  size="small"
                  variant="text"
                  color="#EB0E0E"
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

    <!-- Add Product Dialog -->
    <v-dialog v-model="dialog" max-width="600px" persistent>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon icon="mdi-plus" class="me-3"></v-icon>
          <span class="text-h5">Add New Product</span>
          <v-spacer></v-spacer>
          <v-btn
            icon="mdi-close"
            variant="text"
            @click="closeDialog"
          ></v-btn>
        </v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text class="pt-6">
          <v-form>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="newProduct.productName"
                  label="Product Name"
                  placeholder="Enter product name"
                  variant="outlined"
                  :rules="nameRules"
                  counter="100"
                  maxlength="100"
                  required
                ></v-text-field>
              </v-col>
              
              <v-col cols="12">
                <v-select
                  v-model="newProduct.category"
                  label="Product Type"
                  :items="productTypes"
                  variant="outlined"
                  required
                ></v-select>
              </v-col>
              
            
            </v-row>
          </v-form>
        </v-card-text>
        
        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn
            variant="outlined"
            @click="closeDialog"
            :disabled="saving"
          >
            Cancel
          </v-btn>
          <v-btn
            color="#021828"
            variant="elevated"
            @click="saveProduct"
            :loading="saving"
            :disabled="saving"
          >
            Save Product
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Add Color Dialog -->
    <v-dialog v-model="colorDialog" max-width="600px" persistent>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon icon="mdi-palette" class="me-3"></v-icon>
          <span class="text-h5">Add Color Variant</span>
          <v-spacer></v-spacer>
          <v-btn
            icon="mdi-close"
            variant="text"
            @click="closeColorDialog"
          ></v-btn>
        </v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text class="pt-6">
          <v-form>
            <v-row>
              <v-col cols="12">
                <v-select
                  v-model="newColor.productId"
                  label="Select Product"
                  :items="uniqueProducts"
                  variant="outlined"
                  :rules="productSelectRules"
                  placeholder="Choose a product to add color variant"
                  required
                ></v-select>
              </v-col>
              
              <v-col cols="12" md="6">
                <v-text-field
                  v-model="newColor.colorCode"
                  label="Color Code"
                  placeholder="Enter catalog color code"
                  variant="outlined"
                  :rules="colorRules"
                  required
                ></v-text-field>
              </v-col>
              
              <v-col cols="12" md="6">
                <v-number-input
                  v-model="newColor.stockCount"
                  label="Stock Count"
                  variant="outlined"
                  :rules="stockRules"
                  :min=0
                  :step=1
                ></v-number-input>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        
        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn
            variant="outlined"
            @click="closeColorDialog"
            :disabled="savingColor"
          >
            Close
          </v-btn>
          <v-btn
            color="#DCC0A1"
            variant="elevated"
            @click="saveColor"
            :loading="savingColor"
            :disabled="savingColor"
          >
            Save Color
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

        <!-- Edit Product Dialog -->
    <v-dialog v-model="editDialog" max-width="600px" persistent>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon icon="mdi-pencil" class="me-3"></v-icon>
          <span class="text-h5">Edit Product</span>
          <v-spacer></v-spacer>
          <v-btn
            icon="mdi-close"
            variant="text"
            @click="closeEditDialog"
          ></v-btn>
        </v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text class="pt-6">
          <v-form>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="editProduct.name"
                  label="Product Name"
                  placeholder="Enter product name"
                  variant="outlined"
                  :rules="nameRules"
                  counter="100"
                  maxlength="100"
                  required
                ></v-text-field>
              </v-col>
              
              <v-col cols="12">
                <v-select
                  v-model="editProduct.category"
                  label="Product Type"
                  :items="productTypes"
                  variant="outlined"
                  required
                ></v-select>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        
        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn
            variant="outlined"
            @click="closeEditDialog"
            :disabled="savingEdit"
          >
            Cancel
          </v-btn>
          <v-btn
            color="#021828"
            variant="elevated"
            @click="updateProduct"
            :loading="savingEdit"
            :disabled="savingEdit"
          >
            Update Product
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

        <!-- Delete Confirmation Dialog -->
    <v-dialog v-model="deleteDialog" max-width="500px" persistent>
      <v-card>
        <v-card-title class="d-flex align-center">
          <v-icon icon="mdi-delete" class="me-3" color="#EB0E0E"></v-icon>
          <span class="text-h5">{{ getDeleteTitle() }}</span>
          <v-spacer></v-spacer>
          <v-btn
            icon="mdi-close"
            variant="text"
            @click="closeDeleteDialog"
            :disabled="deleting"
          ></v-btn>
        </v-card-title>
        
        <v-divider></v-divider>
        
        <v-card-text class="pt-6">
          <div class="text-body-1">
            {{ getDeleteMessage() }}
          </div>
          
          <v-alert 
            type="warning" 
            variant="tonal"
            class="mt-4"
          >
            <strong>Warning:</strong> This action cannot be undone.
          </v-alert>
        </v-card-text>
        
        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn
            variant="outlined"
            @click="closeDeleteDialog"
            :disabled="deleting"
          >
            Cancel
          </v-btn>
          <v-btn
            color="#EB0E0E"
            variant="elevated"
            @click="confirmDelete"
            :loading="deleting"
            :disabled="deleting"
          >
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

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
.text-truncate {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>