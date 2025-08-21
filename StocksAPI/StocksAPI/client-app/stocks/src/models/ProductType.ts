export const ProductType = {
    KnittingThreads: 'KnittingThreads',
    Zippers: 'Zippers',
    SewingThreads: 'SewingThreads',
    Ribbons: 'Ribbons',
    Buttons: 'Buttons',
    Laces: 'Laces'
} as const;

export type ProductType = typeof ProductType[keyof typeof ProductType];

export const getProductTypeDisplayName = (type: ProductType): string => {
    const displayNames: Record<ProductType, string> = {
        [ProductType.KnittingThreads]: 'Knitting Threads',
        [ProductType.Zippers]: 'Zippers',
        [ProductType.SewingThreads]: 'Sewing Threads',
        [ProductType.Ribbons]: 'Ribbons',
        [ProductType.Buttons]: 'Buttons',
        [ProductType.Laces]: 'Laces'
    };
    return displayNames[type];
};