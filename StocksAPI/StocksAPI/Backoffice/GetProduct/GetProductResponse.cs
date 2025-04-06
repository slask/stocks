using System.Text.Json.Serialization;

namespace StocksAPI.Backoffice.GetProduct;

public class GetProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ColorInfo>? Colors { get; set; } = new();
}

public class ColorInfo
{
    public Guid ColorId { get; set; }
    public string Code { get; set; }
    public int StockCount { get; set; }
}