namespace StocksAPI.Backoffice.ListOrders;

public class ListOrdersResponse
{
    public List<OrderRow> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class OrderRow
{
    public Guid OrderId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string? CreatedBy { get; set; }
}
