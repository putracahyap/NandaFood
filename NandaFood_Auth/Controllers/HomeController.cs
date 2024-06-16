using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NandaFood_Auth.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HomeController : ControllerBase
{
    public HomeController()
    {
            
    }

    [HttpGet]
    [Authorize(Roles = "NFA")]
    public IActionResult Get()
    {
        return Ok("Welcome to HomeController");
    }
}