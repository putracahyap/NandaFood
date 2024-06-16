namespace NandaFood_Menu.Models.DTO;

public class AddMenuRequest
{
    public string Menu { get; set; } = null!;
    
    public long Price { get; set; }
    
}