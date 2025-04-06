using StocksAPI.Models;

namespace StocksAPI.Backoffice.CreateProduct;

public class CreateProductRequest
{
    public string Name { get; set; }
    public ProductType Category { get; set; }
}