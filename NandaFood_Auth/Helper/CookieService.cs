namespace NandaFood_Auth.Helper;

public interface ICookieService
{
    void SetTokenCookie(HttpResponse response, string key, string token);
}

public class CookieService : ICookieService
{
    public void SetTokenCookie(HttpResponse response, string key, string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        response.Cookies.Append(key, token, cookieOptions);
    }
}