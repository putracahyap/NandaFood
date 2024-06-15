using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;

namespace NandaFood_Auth.Controllers;

public class AuthenticationController : ControllerBase
{
    private readonly NandafoodContext _dbContext;
    
    public AuthenticationController(NandafoodContext context)
    {
        _dbContext = context;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AccountRequest accountRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Please provide all the required fields");
        }

        var userExist = await _dbContext.Accounts.FirstOrDefaultAsync(u => u.Username == accountRequest.Username);

        if (userExist != null)
        {
            return BadRequest($"Username = {accountRequest.Username} already exist.");
        }
        
        Account newUser = new Account()
        {
            Id = Guid.NewGuid().ToString(),
            Username = accountRequest.Username,
            UserSecret = accountRequest.UserSecret,
            UserRole = accountRequest.UserRole,
            FirstName = accountRequest.FirstName,
            LastName = accountRequest.LastName,
            CreatedDate = DateTime.Now
        };
            
        _dbContext.Accounts.Add(newUser);
        await _dbContext.SaveChangesAsync();
            
        return Ok("User Created");
    }
}