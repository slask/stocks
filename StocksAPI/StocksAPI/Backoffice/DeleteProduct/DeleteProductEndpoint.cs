using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.DeleteProduct;

public class DeleteProductEndpoint(StocksDbContext db) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/product/{id}");
        AllowAnonymous();
    }

    public async override Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var product = await db.Products
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product == null)
        {
            ThrowError($"Product with Id {id} not found.");
            return;
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}