namespace StocksAPI.Backoffice.ListOrders;

public class ListOrdersRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
