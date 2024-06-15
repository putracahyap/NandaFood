using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Data;
using NandaFood_Auth.Helper;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;
using NandaFood_Auth.Models.Global;

namespace NandaFood_Auth.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly NandafoodContext _dbContext;
    private readonly JwtTokenHandler _jwtTokenHandler;
    
    public AuthenticationController(NandafoodContext context, JwtTokenHandler jwtTokenHandler)
    {
        _dbContext = context;
        _jwtTokenHandler = jwtTokenHandler;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AccountRequest accountRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Please provide all the required fields");
        }

        var user = await _dbContext.Accounts.FirstOrDefaultAsync(u => u.Username == accountRequest.Username);

        if (user != null)
        {
            return BadRequest($"Username = {accountRequest.Username} already exist.");
        }
        
        Account newUser = new Account()
        {
            Id = Guid.NewGuid().ToString(),
            Username = accountRequest.Username,
            UserSecret = PasswordHasher.HashPassword(accountRequest.UserSecret),
            UserRole = accountRequest.UserRole,
            FirstName = accountRequest.FirstName,
            LastName = accountRequest.LastName,
            CreatedDate = DateTime.Now
        };
            
        _dbContext.Accounts.Add(newUser);
        await _dbContext.SaveChangesAsync();
            
        return Ok(new APIMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "User Created"
        });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Please provide all the required fields");
        }

        var user = await _dbContext.Accounts.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

        if (user != null)
        {
            var userSecretHash = user.UserSecret;
            var verifySecret = PasswordHasher.VerifyPassword(loginRequest.UserSecret, userSecretHash);

            if (verifySecret)
            {
                var token = await _jwtTokenHandler.GenerateJWTTokenAsync(user, null);
                
                return Ok(new APIMessage<object>()
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Status = "OK",
                    Message = "Login Successful",
                    Data = token
                });
            }
        }

        return Unauthorized(new APIMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.Unauthorized,
            Status = "Unauthorized",
            Message = "Username or Password is incorrect!"
        });
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Please provide all the required fields");
        }

        var result = await _jwtTokenHandler.VerifyAndGenerateTokenAsync(refreshTokenRequest);

        return Ok(result);
    }
}