using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;
using StocksAPI.Models;

namespace StocksAPI.Backoffice.CreateProduct;

public class CreateProductEndpoint(StocksDbContext db) : Endpoint<CreateProductRequest, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/product");
        Policies("Admin");
    }

    public async override Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        // Check if a product with the same name already exists
        var productExists = await db.Products
            .AnyAsync(p => p.Name.ToLower() == req.Name.ToLower(), ct);

        if (productExists)
        {
            ThrowError("Product with the same name already exists.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Category = req.Category
        };

        await db.Products.AddAsync(product, ct);
        await db.SaveChangesAsync(ct);

        await SendAsync(new CreateProductResponse
        {
            Id = product.Id
        }, cancellation: ct);
    }
}