using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.GetProduct;

public class GetProductEndpoint(StocksDbContext db) : Endpoint<GetProductRequest, GetProductResponse>
{
    public override void Configure()
    {
        Get("/product/{Id}");
        AllowAnonymous();
    }

    public async override Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var query = db.Products.AsQueryable();
        
        if (req.IncludeColors)
        {
            query = query.Include(p => p.Colors);
        }

        var product = await query.FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        if (product == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var response = new GetProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category.ToString(),
            Colors = null
        };

        if (req.IncludeColors)
        {
            response.Colors = product.Colors.Select(c => new ColorInfo
            {
                ColorId = c.Id,
                Code = c.Code,
                StockCount = c.StockCount
            }).ToList();
        }

        await SendAsync(response, cancellation: ct);
    }
}