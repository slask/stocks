namespace StocksAPI.Models;

public class ProductColor
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public string Code { get; set; }
    public int StockCount { get; set; }
}