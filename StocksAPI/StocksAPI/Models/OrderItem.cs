namespace StocksAPI.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; } 
    public Guid ColorId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public ProductType Category { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
