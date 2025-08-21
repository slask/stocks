import type { ProductType } from "./ProductType"

interface ProductItem {
    productId: string
    colorId: string
    productName: string
    category: ProductType
    colorCode: string
    stockCount: number
}


export type { ProductItem as default };
