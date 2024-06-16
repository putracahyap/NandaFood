using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NandaFood_Auth.Services;
using NandaFood_Menu.Data;
using NandaFood_Menu.Models;
using NandaFood_Menu.Models.DTO;
using NandaFood_Menu.Models.Global;

namespace NandaFood_Menu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController(NandaFoodMenuContext context, JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpGet("get-all-menu")]
    public async Task<IActionResult?> GetAllMenu()
    {
        IEnumerable<FoodMenu> results = await context.FoodMenu.ToListAsync();
        
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
            Message = "Success Get All Data",
            Data = results
        });
    }
    
    [HttpPost("add")]
    [Authorize(Roles = "NFA")]
    public async Task<IActionResult> AddMenu([FromBody] AddMenuRequest addMenuRequest)
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
        
        bool menuExists = await context.FoodMenu.AnyAsync(r => r.Menu == addMenuRequest.Menu);

        if (menuExists)
        {
            return BadRequest(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Status = "Bad Request",
                Message = $"Menu {addMenuRequest.Menu} already exist."
            });
        }
        
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var loggedInUsername = JwtTokenService.GetUsernameFromToken(token);

        FoodMenu newMenu = new FoodMenu()
        {
            Id = Guid.NewGuid().ToString(),
            Menu = addMenuRequest.Menu,
            Price = addMenuRequest.Price,
            Status = false,
            CreatedBy = loggedInUsername,
            CreatedDate = DateTime.Now
        };
            
        context.FoodMenu.Add(newMenu);
        await context.SaveChangesAsync();
            
        return Ok(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Status = "OK",
            Message = "Menu Created"
        });
    }
    
    [HttpPut("update")]
    [Authorize(Roles = "NFA")]
    public async Task<IActionResult> UpdateMenu([FromBody] UpdateMenuRequest updateMenuRequest)
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
        
        FoodMenu? dbMenu = await context.FoodMenu.FirstOrDefaultAsync(u => u.Id == updateMenuRequest.Id);
        string token = JwtTokenService.ExtractTokenFromRequest(HttpContext.Request);
        var loggedInUsername = JwtTokenService.GetUsernameFromToken(token);
        
        if (dbMenu != null)
        {
            bool menuExists = await context.FoodMenu.AnyAsync(r => r.Menu == updateMenuRequest.Menu);

            if (menuExists)
            {
                return BadRequest(new ApiMessage<object>()
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Status = "Bad Request",
                    Message = $"This {updateMenuRequest.Menu} menu is the same as before."
                });
            }
            
            dbMenu.Menu = updateMenuRequest.Menu ?? dbMenu.Menu;
            dbMenu.Price = updateMenuRequest.Price ?? dbMenu.Price;
            dbMenu.Status = updateMenuRequest.Status ?? dbMenu.Status;
            dbMenu.UpdatedBy = loggedInUsername;
            dbMenu.UpdatedDate = DateTime.Now;
            
            await context.SaveChangesAsync();
            
            return Ok(new ApiMessage<object>()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Status = "OK",
                Message = "Menu Updated"
            });
        }
        
        return BadRequest(new ApiMessage<object>()
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Status = "Bad Request",
            Message = "Id not found"
        });
    }
}