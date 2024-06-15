using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;
using NandaFood_Auth.Models.Global;

namespace NandaFood_Auth.Helper;

public class JwtTokenHandler(
    NandafoodContext context,
    IConfiguration configuration,
    TokenValidationParameters tokenValidationParameters)
{
    public async Task<AuthResult> GenerateJwtTokenAsync(Account? existingUser, string? refreshToken)
    {
        var authClaims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, existingUser.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, existingUser.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, existingUser.UserRole)
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"], 
            audience:configuration["JWT:Audience"], 
            expires: DateTime.Now.AddMinutes(1), 
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        if (refreshToken != null)
        {
            var refreshTokenResponse = new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Expired = token.ValidTo,
            };

            return refreshTokenResponse;
        }

        var newRefreshToken = new RefreshToken()
        {
            Id = Guid.NewGuid().ToString(),
            JwtId = token.Id,
            IsRevoked = false,
            AccountsId = existingUser.Id,
            DateAdded = DateTime.Now,
            DateExpire = DateTime.Now.AddMonths(6),
            Token = Guid.NewGuid()+"-"+Guid.NewGuid()
        };

        var dbUser = await context.Accounts.FirstOrDefaultAsync(x => x.Username == existingUser.Username);
        if (dbUser != null)
        {
            dbUser.JwtToken = jwtToken;
        }
        
        await context.RefreshTokens.AddAsync(newRefreshToken);
        await context.SaveChangesAsync();

        var authResponse = new AuthResult()
        {
            Token = jwtToken,
            RefreshToken = newRefreshToken.Token,
            Expired = token.ValidTo,
        };

        return authResponse;
    }
    
    public async Task<AuthResult> VerifyAndGenerateTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var storedToken =
            await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshTokenRequest.RefreshToken);
        var existingUser = await context.Accounts.FirstOrDefaultAsync(x => x.JwtToken == refreshTokenRequest.JwtToken);
    
        try
        {
            jwtTokenHandler.ValidateToken(refreshTokenRequest.JwtToken, tokenValidationParameters,
                out var validatedToken);
    
            return await GenerateJwtTokenAsync(existingUser, refreshTokenRequest.RefreshToken);
        }
        catch (SecurityTokenExpiredException)
        {
            if (storedToken.DateExpire >= DateTime.Now)
            {
                return await GenerateJwtTokenAsync(existingUser, refreshTokenRequest.RefreshToken);
            }
    
            return await GenerateJwtTokenAsync(existingUser, null);
        }
    }
    
    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            // Validate token
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            // Optionally, perform additional validation or checks here
            
            return principal;
        }
        catch (Exception ex)
        {
            // Handle validation errors
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }

    public string ReadToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return jwtToken.Id;
    }
    
    public void RevokeToken(string token)
    {
        var revokedToken = new RevokedToken
        {
            Id = Guid.NewGuid().ToString(),
            Token = token,
            RevocationDate = DateTime.Now
        };

        var dbUser = context.Accounts.FirstOrDefault(x => x.JwtToken == token);
        var refreshToken = context.RefreshTokens.FirstOrDefault(y => y.AccountsId == dbUser.Id);

        refreshToken.IsRevoked = true;

        context.RevokedTokens.Add(revokedToken);
        context.SaveChanges();
    }
    
    public static string ExtractTokenFromRequest(HttpRequest request)
    {
        string? authorizationHeader = request.Headers.Authorization.FirstOrDefault();
        if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer "))
        {
            return authorizationHeader["Bearer ".Length..].Trim();
        }
        
        return "";
    }
}