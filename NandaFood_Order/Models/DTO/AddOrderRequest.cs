namespace NandaFood_Order.Models.DTO;

public class AddOrderRequest
{
    public string Menu { get; set; } = null!;

    public int Quantity { get; set; }
}