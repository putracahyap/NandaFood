using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Data;
using NandaFood_Auth.Helper;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;
using NandaFood_Auth.Models.Global;
using NandaFood_Auth.Services;

namespace NandaFood_Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(
    NandaFoodAuthContext context,
    JwtTokenService jwtTokenService,
    ICookieService cookieService)
    : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] AccountRequest accountRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }
        
        Account? dbUser = await context.Accounts.FirstOrDefaultAsync(u => u.Username == accountRequest.Username);
        
        bool roleExists = await context.Roles.AnyAsync(r => r.RoleCode == accountRequest.UserRole);

        if (dbUser != null)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = $"Username {accountRequest.Username} already exist."
            });
        }

        if (!roleExists)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Role is not available."
            });
        }
        
        Account newUser = new Account()
        {
            Id = Guid.NewGuid().ToString(),
            Username = accountRequest.Username,
            UserSecret = PasswordHasher.HashPassword(accountRequest.UserSecret),
            UserRole = accountRequest.UserRole,
            FirstName = accountRequest.FirstName,
            LastName = accountRequest.LastName,
            CreatedDate = DateTime.Now,
            IsLogin = false
        };
            
        context.Accounts.Add(newUser);
        await context.SaveChangesAsync();
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "User Created"
        });
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }

        var dbUser = await context.Accounts.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

        if (dbUser != null)
        {
            var userSecretHash = dbUser.UserSecret;
            var verifySecret = PasswordHasher.VerifyPassword(loginRequest.UserSecret, userSecretHash);

            if (verifySecret)
            {
                var result = await jwtTokenService.GenerateJwtTokenAsync(dbUser, null);
                
                cookieService.SetTokenCookie(Response, "jwtToken", result.Token);
                cookieService.SetTokenCookie(Response, "refreshToken", result.RefreshToken);
                
                return Ok(new ApiMessage<object>()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = "OK",
                    Message = "Login Successful",
                    Data = result
                });
            }
        }

        return Unauthorized(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.Unauthorized,
            Status = "Unauthorized",
            Message = "Username or Password is incorrect!"
        });
    }
    
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);

        jwtTokenService.RevokeToken(token);

        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Logout successful"
        });
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Please provide all the required fields"
            });
        }
        
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);

        var result = await jwtTokenService.VerifyAndGenerateTokenAsync(refreshTokenRequest, token);
        
        cookieService.SetTokenCookie(Response, "jwtToken", result.Token);
        cookieService.SetTokenCookie(Response, "refreshToken", result.RefreshToken);

        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Token Refreshed",
            Data = result
        });
    }
}