namespace StocksAPI.Backoffice.SaveOrder;

public class SaveOrderResponse
{
    public Guid OrderId { get; set; }
    public string Message { get; set; } = string.Empty;
}
