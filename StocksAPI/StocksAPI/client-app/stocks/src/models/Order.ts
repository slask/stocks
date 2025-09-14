interface Order {
    orderId: string
    clientName: string
    orderDate: Date
    createdBy: string | null
}

export type { Order as default }