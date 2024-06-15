namespace NandaFood_Auth.Models;

public sealed partial class Role
{
    public string RoleCode { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
