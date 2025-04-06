using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.EditProduct;

public class EditProductEndpoint(StocksDbContext db) : Endpoint<EditProductRequest, EditProductResponse>
{
    public override void Configure()
    {
        Put("/api/product/{Id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(EditProductRequest req, CancellationToken ct)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        if (product == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        product.Name = req.Name;
        product.Category = req.Category;

        await db.SaveChangesAsync(ct);

        await SendAsync(new EditProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category.ToString()
        }, cancellation: ct);
    }
}