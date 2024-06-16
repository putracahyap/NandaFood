namespace NandaFood_Auth.Models.DTO;

public class UpdateAccountRequest
{
    public string? UserRole { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}