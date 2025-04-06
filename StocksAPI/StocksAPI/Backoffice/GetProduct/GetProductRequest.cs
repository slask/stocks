using Microsoft.AspNetCore.Mvc;

namespace StocksAPI.Backoffice.GetProduct;

public class GetProductRequest
{
    [FromRoute] public Guid Id { get; set; }
    public bool IncludeColors { get; set; } = false;
}