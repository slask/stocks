using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.ListProducts;

public class ListProductsEndpoint(StocksDbContext db) : EndpointWithoutRequest<ListProductsResponse>
{
    public override void Configure()
    {
        Get("products");
        Policies("Employee");
    }

    public async override Task HandleAsync(CancellationToken ct)
    {
        var products = await db.Products
            .Include(p => p.Colors).ToListAsync(cancellationToken: ct);
        var productItems = products.SelectMany(product => product.Colors.Any()
            ? product.Colors.Select(color => new ProductItem
            {
                ProductId = product.Id,
                ColorId = color.Id,
                ProductName = product.Name,
                Category = product.Category.ToString(),
                ColorCode = color.Code,
                StockCount = color.StockCount
            })
            : new List<ProductItem>
            {
                new()
                {
                    ColorId = null,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Category = product.Category.ToString(),
                    ColorCode = null,
                    StockCount = 0
                }
            }).ToList();

        await SendAsync(new ListProductsResponse
        {
            Items = productItems
        }, cancellation: ct);
    }
}