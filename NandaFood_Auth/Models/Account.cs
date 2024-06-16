using System.ComponentModel.DataAnnotations.Schema;

namespace NandaFood_Auth.Models;

public sealed class Account
{
    public string Id { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string UserSecret { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? JwtToken { get; set; }

    public bool IsLogin { get; set; }

    public Role UserRoleNavigation { get; set; } = null!;
}
