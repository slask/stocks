using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.EditProduct;

public class EditProductEndpoint(StocksDbContext db, ILogger<EditProductEndpoint> logger) : Endpoint<EditProductRequest, EditProductResponse>
{
    public override void Configure()
    {
        Put("/product/{Id}");
        Policies("Admin");
    }

    public async override Task HandleAsync(EditProductRequest req, CancellationToken ct)
    {
        var product = await db.Products
            .Include(p => p.Colors)
            .FirstOrDefaultAsync(p => p.Id == req.Id, ct);

        if (product == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        product.Name = req.Name;
        product.Category = req.Category;
        
        // Handle stock quantity update if ColorId is provided
        if (req.ColorId.HasValue)
        {
            var color = product.Colors.FirstOrDefault(c => c.Id == req.ColorId.Value);
            
            if (color == null)
            {
                ThrowError("Color not found for this product.");
                return;
            }

            // Add the quantity to existing stock
            var newQuantity = color.StockCount + req.QuantityToAdd;
            logger.LogInformation("Updating stock for ColorId {ColorId}: Existing={Existing}, ToAdd={ToAdd}, New={New}",
                color.Id, color.StockCount, req.QuantityToAdd, newQuantity);
            
            // Ensure stock doesn't go below 0
            color.StockCount = Math.Max(0, newQuantity);
        }


        await db.SaveChangesAsync(ct);

        await SendAsync(new EditProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category.ToString()
        }, cancellation: ct);
    }
}