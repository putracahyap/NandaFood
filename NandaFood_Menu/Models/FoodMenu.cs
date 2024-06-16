namespace NandaFood_Menu.Models;

public partial class FoodMenu
{
    public string Id { get; set; } = null!;

    public string Menu { get; set; } = null!;

    public long Price { get; set; }

    public bool Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
