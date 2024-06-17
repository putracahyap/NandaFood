using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Data;
using NandaFood_Auth.Models;
using NandaFood_Auth.Models.DTO;
using NandaFood_Auth.Models.Global;
using NandaFood_Auth.Services;

namespace NandaFood_Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(NandaFoodAuthContext context) : ControllerBase
{
    [HttpGet("get-all-accounts")]
    public async Task<IActionResult?> GetAllAccounts()
    {
        IEnumerable<Account> results = await context.Accounts.ToListAsync();
        
        if (!results.Any())
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = "Data Empty"
            });
        }
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Success Get All Account",
            Data = results
        });
    }
    
    [HttpPut("update")]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest updateAccountRequest)
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
        var tokenDetails = JwtTokenService.GetTokenDetails(token);
        Account? dbUser = await context.Accounts.FirstOrDefaultAsync(u => u.Username == tokenDetails.Item1);
        
        if (dbUser == null)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = $"Username {tokenDetails.Item1} does not exist."
            });
        }
    
        dbUser.UserRole = updateAccountRequest.UserRole ?? dbUser.UserRole;
        dbUser.FirstName = updateAccountRequest.FirstName ?? dbUser.FirstName;
        dbUser.LastName = updateAccountRequest.LastName ?? dbUser.LastName;
        dbUser.UpdatedDate = DateTime.Now;
        
        await context.SaveChangesAsync();
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Account Updated"
        });
    }
    
}