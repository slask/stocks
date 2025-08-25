namespace StocksAPI.Backoffice.DeleteColor;

using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Data;

public class DeleteColorEndpoint(StocksDbContext db) : Endpoint<DeleteColorRequest>
{
    public override void Configure()
    {
        Delete("/product/{ProductId}/colors/{ColorId}");
        Policies("Admin");
    }

    public async override Task HandleAsync(DeleteColorRequest req, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);

        if (product == null)
        {
            ThrowError("Product not found.");
            return;
        }

        var color = product.Colors.FirstOrDefault(c => c.Id == req.ColorId);

        if (color == null)
        {
            ThrowError("Color not found.");
            return;
        }

        product.Colors.Remove(color);
        db.Remove(color); // This removes the color from the database
        await db.SaveChangesAsync(ct);

        await SendNoContentAsync(ct);
    }
}