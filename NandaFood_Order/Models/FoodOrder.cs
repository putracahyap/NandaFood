namespace NandaFood_Order.Models;

public partial class FoodOrder
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Menu { get; set; } = null!;

    public int Quantity { get; set; }

    public long TotalPrice { get; set; }

    public string OrderBy { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public string? OrderStatus { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
