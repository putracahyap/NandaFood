namespace NandaFood_Auth.Models;

public partial class RevokedToken
{
    public string Id { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime RevocationDate { get; set; }
}
