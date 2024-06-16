namespace NandaFood_Menu.Models.DTO;

public class UpdateMenuRequest
{
    public string Id { get; set; } = null!;
    public string? Menu { get; set; }
    public long? Price { get; set; }
    public bool? Status { get; set; }
}