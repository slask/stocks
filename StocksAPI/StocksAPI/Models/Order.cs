
namespace StocksAPI.Models;

public class Order
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    public string? CreatedBy { get; set; }
}
