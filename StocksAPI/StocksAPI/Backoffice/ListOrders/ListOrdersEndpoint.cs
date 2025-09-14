using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using StocksAPI.Data;

namespace StocksAPI.Backoffice.ListOrders;

public class ListOrdersEndpoint(StocksDbContext db) : Endpoint<ListOrdersRequest, ListOrdersResponse>
{
    public override void Configure()
    {
        Get("/orders");
        Policies("Employee");
    }

    public async override Task HandleAsync(ListOrdersRequest req, CancellationToken ct)
    {
        // Validate pagination parameters
        if (req.PageNumber < 1)
            req.PageNumber = 1;
        
        if (req.PageSize < 1)
            req.PageSize = 10;
        
        if (req.PageSize > 100)
            req.PageSize = 100;

        // Get total count for pagination
        var totalCount = await db.Orders.CountAsync(ct);
        
        // Calculate pagination
        var skip = (req.PageNumber - 1) * req.PageSize;
        var totalPages = (int)Math.Ceiling((double)totalCount / req.PageSize);

        // Get orders with pagination
        var orders = await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(req.PageSize)
            .Select(o => new OrderRow
            {
                OrderId = o.Id,
                ClientName = o.ClientName,
                OrderDate = o.CreatedAt,
                CreatedBy = o.CreatedBy
            })
            .ToListAsync(ct);

        await SendAsync(new ListOrdersResponse
        {
            Items = orders,
            TotalCount = totalCount,
            PageNumber = req.PageNumber,
            PageSize = req.PageSize,
            TotalPages = totalPages
        }, cancellation: ct);
    }
}
