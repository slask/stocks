interface Order {
    orderId: string
    clientName: string
    orderDate: Date
    createdBy: string | null
}

interface OrderRow {
    productId: string
    productName: string
    category: string
    colorCode: string
    quantity: number
}

export type { Order, OrderRow }