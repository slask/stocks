using FastEndpoints;
using StocksAPI.Data;
using StocksAPI.Models;
using Order = StocksAPI.Models.Order;

namespace StocksAPI.Backoffice.SaveOrder;

public class SaveOrderEndpoint(StocksDbContext db) : Endpoint<SaveOrderRequest, SaveOrderResponse>
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

        // Convert request items to order items
        foreach (var itemDto in req.Items)
        {
            // Parse the category from string to enum
            if (!Enum.TryParse<ProductType>(itemDto.Category, out var category))
            {
                ThrowError($"Invalid category: {itemDto.Category}");
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
        }

        // Save to database
        await db.Orders.AddAsync(order, ct);
        await db.SaveChangesAsync(ct);

        // Return response
        await SendOkAsync(new SaveOrderResponse
        {
            OrderId = order.Id,
            Message = "Order saved successfully"
        }, ct);
    }
}
