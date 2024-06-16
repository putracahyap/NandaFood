using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;
using NandaFood_Auth.Models.Global;

namespace NandaFood_Auth.Services;

public class JwtTokenService(
    NandaFoodAuthContext context,
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
        var newRefreshToken = string.Empty;

        if (refreshToken == null)
        {
            var newRefreshTokenData = new RefreshToken()
            {
                Id = Guid.NewGuid().ToString(),
                Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
                JwtId = token.Id,
                IsRevoked = false,
                AccountsId = existingUser.Id,
                DateAdded = DateTime.Now,
                DateExpire = DateTime.Now.AddMonths(1),
            };

            newRefreshToken = newRefreshTokenData.Token;
        
            await context.RefreshTokens.AddAsync(newRefreshTokenData);
        }
        
        var dbUser = await context.Accounts.FirstOrDefaultAsync(x => x.Id == existingUser.Id);
        
        if (dbUser != null)
        {
            dbUser.IsLogin = true;
        }
        
        var authResponse = new AuthResult()
        {
            Token = jwtToken,
            RefreshToken = refreshToken ?? newRefreshToken,
            Expired = token.ValidTo,
        };
        
        await context.SaveChangesAsync();

        return authResponse;
    }
    
    public async Task<AuthResult> VerifyAndGenerateTokenAsync(RefreshTokenRequest refreshTokenRequest, string jwtToken)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var storedToken =
            await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshTokenRequest.RefreshToken);
        var loggedInUsername = GetUsernameFromToken(jwtToken);
        var existingUser = await context.Accounts.FirstOrDefaultAsync(x => x.Username == loggedInUsername);
    
        try
        {
            jwtTokenHandler.ValidateToken(jwtToken, tokenValidationParameters,
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
    
    public void RevokeToken(string token)
    {
        var revokedToken = new RevokedToken
        {
            Id = Guid.NewGuid().ToString(),
            Token = token,
            RevocationDate = DateTime.Now
        };

        var loggedInUsername = GetUsernameFromToken(token);
        var dbUser = context.Accounts.FirstOrDefault(x => x.Username == loggedInUsername);
        var refreshToken = context.RefreshTokens.FirstOrDefault(y => y.AccountsId == dbUser.Id);

        dbUser.IsLogin = false;
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
    
    public static string GetUsernameFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName).Value;

        return username;
    }
}