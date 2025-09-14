namespace StocksAPI.Backoffice.OrderDetails;

public class OrderDetailsResponse
{
    public List<OrderItemDetail> Items { get; set; } = new();
}

public class OrderItemDetail
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
