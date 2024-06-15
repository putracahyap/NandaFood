using System.ComponentModel.DataAnnotations;

namespace NandaFood_Auth.Models.DTO;

public class LoginRequest
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string UserSecret { get; set; }
}