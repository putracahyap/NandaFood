using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;

namespace NandaFood_Auth.Helper;

public class JwtTokenHandler
{
    private readonly IConfiguration _configuration;
    private readonly NandafoodContext _dbContext;
    private readonly TokenValidationParameters _tokenValidationParameters;
    
    public JwtTokenHandler(NandafoodContext context, IConfiguration configuration, TokenValidationParameters tokenValidationParameters)
    {
        _dbContext = context;
        _configuration = configuration;
        _tokenValidationParameters = tokenValidationParameters;
    }
    
    public async Task<AuthResult> GenerateJWTTokenAsync(Account existingUser, string? refreshToken)
    {
        // Create Jwt Claim
        var authClaims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, existingUser.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        // Get SigningKey
        var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

        // Create Jwt Security Token
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"], 
            audience:_configuration["JWT:Audience"], 
            expires: DateTime.Now.AddMinutes(1), 
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

        // Create Jwt Token
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Check if refreshToken is null or not null
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

        // Add new RefreshToken
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

        // Update Jwt Token in Logged In Accounts
        var dbUser = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Username == existingUser.Username);
        if (dbUser != null)
        {
            dbUser.JwtToken = jwtToken;
        }
        
        // Add new row for RefreshToken in database and save it
        await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
        await _dbContext.SaveChangesAsync();

        // Create New AuthResult
        var authResponse = new AuthResult()
        {
            Token = jwtToken,
            RefreshToken = newRefreshToken.Token,
            Expired = token.ValidTo,
        };

        // Return Auth Response
        return authResponse;
    }
    
    public async Task<AuthResult> VerifyAndGenerateTokenAsync(RefreshTokenRequest refreshTokenRequest)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();
        var storedToken =
            await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshTokenRequest.RefreshToken);
        var existingUser = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.JwtToken == refreshTokenRequest.JwtToken);
    
        try
        {
            jwtTokenHandler.ValidateToken(refreshTokenRequest.JwtToken, _tokenValidationParameters,
                out var validatedToken);
    
            return await GenerateJWTTokenAsync(existingUser, refreshTokenRequest.RefreshToken);
        }
        catch (SecurityTokenExpiredException)
        {
            if (storedToken.DateExpire >= DateTime.Now)
            {
                return await GenerateJWTTokenAsync(existingUser, refreshTokenRequest.RefreshToken);
            }
    
            return await GenerateJWTTokenAsync(existingUser, null);
        }
    }
}