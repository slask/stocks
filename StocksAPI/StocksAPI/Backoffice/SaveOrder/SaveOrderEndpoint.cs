using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;
using StocksAPI.Models;
using Order = StocksAPI.Models.Order;

namespace StocksAPI.Backoffice.SaveOrder;

public class SaveOrderEndpoint(StocksDbContext db, ILogger<SaveOrderEndpoint> logger) : Endpoint<SaveOrderRequest, SaveOrderResponse>
{
    public override void Configure()
    {
        Post("/orders");
        AllowAnonymous();
    }

    public async override Task HandleAsync(SaveOrderRequest req, CancellationToken ct)
    {
        // Create the order entity
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ClientName = req.ClientName,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        // Convert request items to order items and validate they exist in stock
        var stockUpdates = new List<(Guid ProductId, Guid ColorId, int Quantity)>();

        foreach (var itemDto in req.Items)
        {
            // Parse the category from string to enum
            if (!Enum.TryParse<ProductType>(itemDto.Category, out var category))
            {
                ThrowError($"Invalid category: {itemDto.Category}");
                return;
            }

            // Parse ProductId and ColorId from strings to Guids
            if (!Guid.TryParse(itemDto.ProductId, out var productId))
            {
                ThrowError($"Invalid product ID format: {itemDto.ProductId}");
                return;
            }

            if (!Guid.TryParse(itemDto.ColorId, out var colorId))
            {
                ThrowError($"Invalid color ID format: {itemDto.ColorId}");
                return;
            }

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = itemDto.ProductId,
                ColorId = itemDto.ColorId,
                ProductName = itemDto.ProductName,
                Category = category,
                ColorCode = itemDto.ColorCode,
                Quantity = itemDto.Quantity
            };

            order.Items.Add(orderItem);
            stockUpdates.Add((productId, colorId, itemDto.Quantity));
        }

        // Update stock quantities
        foreach (var (productId, colorId, quantity) in stockUpdates)
        {
            var product = await db.Products
                .Include(p => p.Colors)
                .FirstOrDefaultAsync(p => p.Id == productId, ct);

            if (product == null)
            {
                ThrowError($"Product with ID {productId} not found in stock.");
                return;
            }

            var color = product.Colors.FirstOrDefault(c => c.Id == colorId);
            if (color == null)
            {
                ThrowError($"Color with ID {colorId} not found for product {product.Name}.");
                return;
            }

            // Check if there's sufficient stock
            if (color.StockCount < quantity)
            {
                logger.LogWarning("Insufficient stock for {ProductName} (Color: {ColorCode}). Available: {ColorStockCount}, Requested: {Quantity}", product.Name, color.Code, color.StockCount, quantity);
                color.StockCount = 0;
            }
            else
            {
                // Subtract the ordered quantity from stock
                color.StockCount -= quantity;
            }
        }

        // Save to database
        await db.Orders.AddAsync(order, ct);
        await db.SaveChangesAsync(ct);

        // Return response
        await SendOkAsync(new SaveOrderResponse
        {
            OrderId = order.Id,
            Message = "Order saved successfully and stock updated"
        }, ct);
    }
}
