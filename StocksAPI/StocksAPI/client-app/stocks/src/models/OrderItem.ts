import type { ProductType } from './ProductType'

interface OrderItem {
  productId: string
  colorId: string
  productName: string
  category: ProductType
  colorCode: string
  quantity: number
}

export type { OrderItem as default }
