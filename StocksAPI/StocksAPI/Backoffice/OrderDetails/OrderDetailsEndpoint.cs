using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.OrderDetails;

public class OrderDetailsEndpoint(StocksDbContext db) : Endpoint<OrderDetailsRequest, OrderDetailsResponse>
{
    public override void Configure()
    {
        Get("/orders/{OrderId}");
        Policies("Employee");
    }

    public async override Task HandleAsync(OrderDetailsRequest req, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);

        if (order == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var orderItems = order.Items.Select(item => new OrderItemDetail
        {
            Id = item.Id,
            ProductName = item.ProductName,
            ColorCode = item.ColorCode,
            Category = item.Category.ToString(),
            Quantity = item.Quantity
        }).ToList();

        await SendAsync(new OrderDetailsResponse
        {
            Items = orderItems
        }, cancellation: ct);
    }
}
