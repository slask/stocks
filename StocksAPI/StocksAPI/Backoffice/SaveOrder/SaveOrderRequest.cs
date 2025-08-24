namespace StocksAPI.Backoffice.SaveOrder;

public class SaveOrderRequest
{
    public string ClientName { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public string ProductId { get; set; } = string.Empty;
    public string ColorId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
