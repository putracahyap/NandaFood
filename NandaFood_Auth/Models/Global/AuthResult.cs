namespace NandaFood_Auth.Models.Global;

public class AuthResult
{
    public string Token { get; set; }
    public DateTime Expired { get; set; }

    public string RefreshToken { get; set; }
}