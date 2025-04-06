namespace StocksAPI.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ProductType Category { get; set; }
    public List<ProductColor> Colors { get; set; } = new();
}