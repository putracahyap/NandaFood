namespace NandaFood_Order.Models.DTO;

public class UpdateStatusRequest
{
    public string Id { get; set; } = null!;
    public string Status { get; set; } = null!;
}