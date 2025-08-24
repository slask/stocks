using System.ComponentModel.DataAnnotations;

namespace StocksAPI.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string ProductId { get; set; } = string.Empty;
    public string ColorId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public ProductType Category { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
