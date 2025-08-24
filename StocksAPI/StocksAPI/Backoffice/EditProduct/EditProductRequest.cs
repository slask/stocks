using Microsoft.AspNetCore.Mvc;
using StocksAPI.Models;

namespace StocksAPI.Backoffice.EditProduct;

public class EditProductRequest
{
    [FromRoute] public Guid Id { get; set; }
    public string Name { get; set; }
    public ProductType Category { get; set; }
    
    public Guid? ColorId { get; set; }
    
    public int QuantityToAdd { get; set; }
}