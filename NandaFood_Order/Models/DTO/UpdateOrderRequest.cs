namespace NandaFood_Order.Models.DTO;

public class UpdateOrderRequest
{
    public string Id { get; set; } = null!;
    public string? Menu { get; set; }

    public int? Quantity { get; set; }

}