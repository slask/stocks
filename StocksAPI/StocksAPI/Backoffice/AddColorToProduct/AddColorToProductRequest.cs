using Microsoft.AspNetCore.Mvc;

namespace StocksAPI.Backoffice.AddColorToProduct;

public class AddColorToProductRequest
{
    [FromRoute] public Guid ProductId { get; set; }

    public string Code { get; set; }
    public int ExistingQuantity { get; set; }
}