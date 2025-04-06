using Microsoft.AspNetCore.Mvc;

namespace StocksAPI.Backoffice.DeleteColor;

public class DeleteColorRequest
{
    [FromRoute] public Guid ProductId { get; set; }
    [FromRoute] public Guid ColorId { get; set; }
}