using System.ComponentModel.DataAnnotations.Schema;

namespace NandaFood_Auth.Models;

public partial class Account
{
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string UserSecret { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [Column(TypeName = "varchar(max)")]
    public string? JwtToken { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    public virtual Role UserRoleNavigation { get; set; } = null!;
}
