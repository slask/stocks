namespace StocksAPI.Backoffice.ListProducts;

public class ListProductsResponse
{
    public List<ProductItem> Items { get; set; } = new();
}

public class ProductItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public string ColorCode { get; set; }
    public int StockCount { get; set; }
}