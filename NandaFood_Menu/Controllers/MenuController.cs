using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NandaFood_Menu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    public MenuController()
    {
            
    }
    
    [HttpGet]
    [Authorize(Roles = "NFA")]
    public IActionResult Get()
    {
        return Ok("Welcome to MenuController");
    }
}