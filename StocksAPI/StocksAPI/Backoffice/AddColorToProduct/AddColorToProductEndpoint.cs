using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;
using StocksAPI.Models;

namespace StocksAPI.Backoffice.AddColorToProduct;

public class AddColorToProductEndpoint(StocksDbContext db)
    : Endpoint<AddColorToProductRequest, AddColorToProductResponse>
{
    public override void Configure()
    {
        Post("/product/{productId}/colors");
        AllowAnonymous();
    }

    public async override Task HandleAsync(AddColorToProductRequest req, CancellationToken ct)
    {
        // Find the product
        var product = await db.Products
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == req.ProductId, ct);

        if (product == null)
        {
            ThrowError("Product not found.");
            return;
        }

        // Check if color already exists for this product
        if (product.Colors.Any(c => c.Code.ToLower() == req.Code.ToLower()))
        {
            ThrowError($"Color with code '{req.Code}' already exists for this product.");
            return;
        }

        // Create new color
        var color = new ProductColor
        {
            Code = req.Code,
            StockCount = req.ExistingQuantity
        };

        product.Colors.Add(color);
        await db.SaveChangesAsync(ct);

        await SendAsync(new AddColorToProductResponse
        {
            ColorId = color.Id
        }, cancellation: ct);
    }
}