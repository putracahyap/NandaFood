using System.ComponentModel.DataAnnotations;

namespace NandaFood_Auth.Models.DTO;

public class AccountRequest
{
    [Required]
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string UserSecret { get; set; }
    [Required]
    public int UserRole { get; set; }
}