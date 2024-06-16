using System.ComponentModel.DataAnnotations;

namespace NandaFood_Auth.Models.DTO;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; }
}