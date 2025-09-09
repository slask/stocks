<script setup lang="ts">
  import { ref, onMounted } from 'vue'
  import { useApi } from '../api'
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

  const { apiGet, apiPost, apiPut, apiDelete } = useApi()

  // Form data
  const newProduct = ref({
    productName: '',
    category: ProductType.KnittingThreads as ProductType,
  })

  // Color form data
  const newColor = ref({
    productId: '',
    colorCode: '',
    stockCount: null,
  })

  const editProduct = ref({
    id: '',
    name: '',
    category: ProductType.KnittingThreads as ProductType,
    colorId: '',
    quantityToAdd: null,
    hasColor: false,
    currentStock: 0,
  })

  // Form validation rules
  const nameRules = [
    (v: string) => !!v || 'Product name is required',
    (v: string) => v.length >= 2 || 'Product name must be at least 2 characters',
    (v: string) => v.length <= 100 || 'Product name must be less than 100 characters',
  ]

  const colorRules = [(v: string) => !!v || 'Color code is required']

  const stockRules = [
    (v: number | null) => (v !== null && v !== undefined) || 'Stock count is required',
    (v: number | null) => v === null || v >= 0 || 'Stock count must be 0 or greater',
  ]

  const productSelectRules = [(v: string) => !!v || 'Please select a product']

  const quantityRules = [
    (v: number | null) => (v !== null && v !== undefined) || 'Quantity is required',
    (v: number | null) => v === null || Number.isInteger(v) || 'Quantity must be a whole number',
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
      sortable: true,
    },
    {
      title: 'Category',
      key: 'category',
      sortable: true,
    },
    {
      title: 'Color',
      key: 'colorCode',
      sortable: true,
      width: '120px',
    },
    {
      title: 'Stock Count',
      key: 'stockCount',
      sortable: true,
      width: '100px',
    },
    {
      title: 'Actions',
      key: 'actions',
      sortable: false,
      width: '150px',
    },
  ] as const

  // Get ProductType values for dropdown
  const productTypes = Object.values(ProductType).map(type => ({
    value: type,
    title: getProductTypeDisplayName(type),
  }))

  // Get unique products for color dropdown
  const uniqueProducts = ref<Array<{ value: string; title: string }>>([])

  const updateUniqueProducts = () => {
    const productMap = new Map()
    products.value.forEach(product => {
      if (!productMap.has(product.productName)) {
        productMap.set(product.productName, {
          value: product.productId,
          title: product.productName,
        })
      }
    })
    uniqueProducts.value = Array.from(productMap.values())
  }

  const fetchProducts = async () => {
    loading.value = true
    try {
      const response = await apiGet('/api/products')
      products.value = response.data.items as ProductItem[]
      updateUniqueProducts()
    } catch (error: any) {
      console.error('Error fetching products:', error)
      if (error.response?.status === 403) {
        showNotification('You do not have permission to view products.', 'error')
        return
      }
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
      stockCount: null,
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
      const response = await apiPost('/api/product', {
        name: newProduct.value.productName,
        category: newProduct.value.category,
      })
      console.log('Save Product response:', response.data)
      showNotification('Product saved successfully!', 'success')
      closeDialog()

      // Refresh the list
      await fetchProducts()
    } catch (error: any) {
      console.error('Error saving product:', error)
      if (error.status === 403) {
        showNotification('You do not have permission to perform this action.', 'error')
        return
      }
      const errorMessage =
        error.response?.data?.errors.generalErrors[0] ||
        error.response?.data?.message ||
        error.message ||
        'Failed to save product'
      showNotification(`Error: ${errorMessage}`, 'error')
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

    if (newColor.value.stockCount === null || newColor.value.stockCount < 0) {
      showNotification('Stock count must be 0 or greater', 'error')
      return
    }

    savingColor.value = true
    try {
      const response = await apiPost(`/api/product/${newColor.value.productId}/colors`, {
        code: newColor.value.colorCode,
        existingQuantity: newColor.value.stockCount,
      })
      console.log('Save Color variant response:', response.data)
      showNotification('Color variant saved successfully!', 'success')

      // Clear only color and stock fields, keep product selected
      newColor.value = {
        productId: newColor.value.productId,
        colorCode: '',
        stockCount: null,
      }

      await fetchProducts()
    } catch (error: any) {
      console.error('Error saving color variant:', error)
      if (error.response?.status === 403) {
        showNotification('You do not have permission to add color variants.', 'error')
        return
      }
      const errorMessage =
        error.response?.data?.errors.generalErrors[0] ||
        error.response?.data?.message ||
        error.message ||
        'Failed to save color variant'
      showNotification(`Error: ${errorMessage}`, 'error')
    } finally {
      savingColor.value = false
    }
  }

  const showNotification = (message: string, color: 'success' | 'error') => {
    snackbarText.value = message
    snackbarColor.value = color === 'error' ? '#EB0E0E' : '#21603D'
    snackbar.value = true
  }

  const openEditDialog = (product: ProductItem) => {
    const hasColor = itemHasColor(product)

    // Set form data from selected product
    editProduct.value = {
      id: product.productId,
      name: product.productName,
      category: product.category as ProductType,
      colorId: hasColor ? product.colorId : '',
      quantityToAdd: null,
      hasColor: hasColor,
      currentStock: product.stockCount,
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

    // Validate quantity if product has color
    if (
      editProduct.value.hasColor &&
      (editProduct.value.quantityToAdd === null || editProduct.value.quantityToAdd === undefined)
    ) {
      showNotification('Please enter a quantity to add/remove', 'error')
      return
    }

    savingEdit.value = true
    try {
      const requestData: any = {
        name: editProduct.value.name,
        category: editProduct.value.category,
      }

      // Add color-related fields if product has color
      if (editProduct.value.hasColor) {
        requestData.colorId = editProduct.value.colorId
        requestData.quantityToAdd = editProduct.value.quantityToAdd
      }

      const response = await apiPut(`/api/product/${editProduct.value.id}`, requestData)
      console.log('Update response:', response)
      showNotification('Product updated successfully!', 'success')
      closeEditDialog()

      // Refresh the list
      await fetchProducts()
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.errors.name[0] ||
        error.response?.data?.message ||
        error.message ||
        'Failed to update product'
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
    const hasColor = itemHasColor(product)

    deleting.value = true
    try {
      let response
      if (hasColor) {
        response = await apiDelete(`/api/product/${product.productId}/colors/${product.colorId}`)
        showNotification('Color variant deleted successfully!', 'success')
      } else {
        response = await apiDelete(`/api/product/${product.productId}`)
        showNotification('Product deleted successfully!', 'success')
      }
      console.log('Delete response:', response.data)
      closeDeleteDialog()

      await fetchProducts()
    } catch (error: any) {
      console.error('Error deleting item:', error)
      if (error.response?.status === 403) {
        showNotification('You do not have permission to delete items.', 'error')
        return
      }
      const errorMessage =
        error.response?.data?.errors?.generalErrors?.[0] ||
        error.response?.data?.message ||
        error.message ||
        'Failed to delete item'
      showNotification(`Error: ${errorMessage}`, 'error')
    } finally {
      deleting.value = false
    }
  }

  const getDeleteMessage = () => {
    if (!itemToDelete.value) return ''

    const product = itemToDelete.value
    const hasColor = itemHasColor(product)

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

  const itemHasColor = (product: ProductItem): boolean => {
    return !!(product.colorCode && product.colorCode.trim() !== '' && product.colorId)
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
          <v-card-title class="d-flex align-center pe-2 flex-wrap">
            <v-icon icon="mdi-package-variant" class="me-3"></v-icon>
            <span class="text-h5 flex-grow-1">Product Management</span>

            <div class="d-flex flex-wrap gap-3 mt-2 mt-sm-0">
              <v-btn
                color="#021828"
                prepend-icon="mdi-plus"
                variant="elevated"
                @click="openAddDialog"
                size="small"
                class="flex-shrink-0 mr-5"
              >
                Add Product
              </v-btn>

              <v-btn
                color="#DCC0A1"
                prepend-icon="mdi-palette"
                variant="elevated"
                @click="openAddColorDialog"
                size="small"
                class="flex-shrink-0"
              >
                Add Color
              </v-btn>
            </div>
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
                    <span v-bind="props" class="text-truncate d-inline-block" style="max-width: 200px">
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
                <v-chip color="#C6C6C6" size="small" variant="elevated" class="text-black">
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
                  ></v-chip>
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
                ></v-btn>
                <v-btn
                  icon="mdi-delete"
                  size="small"
                  variant="text"
                  color="#EB0E0E"
                  @click="deleteProduct(item)"
                ></v-btn>
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
          <v-btn icon="mdi-close" variant="text" @click="closeDialog"></v-btn>
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
          <v-btn variant="outlined" @click="closeDialog" :disabled="saving">Cancel</v-btn>
          <v-btn color="#021828" variant="elevated" @click="saveProduct" :loading="saving" :disabled="saving">
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
          <v-btn icon="mdi-close" variant="text" @click="closeColorDialog"></v-btn>
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
                  :min="0"
                  :step="1"
                  placeholder="Enter initial stock count"
                ></v-number-input>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn variant="outlined" @click="closeColorDialog" :disabled="savingColor">Close</v-btn>
          <v-btn color="#DCC0A1" variant="elevated" @click="saveColor" :loading="savingColor" :disabled="savingColor">
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
          <v-btn icon="mdi-close" variant="text" @click="closeEditDialog"></v-btn>
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

              <!-- Stock Adjustment Field - only show if product has color -->
              <v-col v-if="editProduct.hasColor" cols="12">
                <v-alert type="info" variant="tonal" class="mb-4">
                  <strong>Current Stock:</strong>
                  {{ editProduct.currentStock }} items
                </v-alert>

                <v-number-input
                  v-model="editProduct.quantityToAdd"
                  label="Quantity to Add/Remove"
                  placeholder="Enter quantity to add (negative to remove)"
                  variant="outlined"
                  :rules="quantityRules"
                  :step="1"
                  hint="Use negative values to remove stock (e.g., -5 to remove 5 items)"
                  persistent-hint
                ></v-number-input>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>

        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn variant="outlined" @click="closeEditDialog" :disabled="savingEdit">Cancel</v-btn>
          <v-btn color="#021828" variant="elevated" @click="updateProduct" :loading="savingEdit" :disabled="savingEdit">
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
          <v-btn icon="mdi-close" variant="text" @click="closeDeleteDialog" :disabled="deleting"></v-btn>
        </v-card-title>

        <v-divider></v-divider>

        <v-card-text class="pt-6">
          <div class="text-body-1">
            {{ getDeleteMessage() }}
          </div>

          <v-alert type="warning" variant="tonal" class="mt-4">
            <strong>Warning:</strong>
            This action cannot be undone.
          </v-alert>
        </v-card-text>

        <v-card-actions class="px-6 pb-6">
          <v-spacer></v-spacer>
          <v-btn variant="outlined" @click="closeDeleteDialog" :disabled="deleting">Cancel</v-btn>
          <v-btn color="#EB0E0E" variant="elevated" @click="confirmDelete" :loading="deleting" :disabled="deleting">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Notification Snackbar -->
    <v-snackbar v-model="snackbar" :color="snackbarColor" timeout="4000" location="top right">
      {{ snackbarText }}
      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar = false">Close</v-btn>
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
