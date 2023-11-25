using CRM.Models;
using Microsoft.AspNetCore.Mvc;
using CRM.Auth;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private IConfiguration _config;
    public LoginController(IConfiguration config) 
    {
        _config = config;
    }

    // [HttpPost]
    // public IActionResult Login(User user)
    // {
    //     //your logic for login process
    //     //If login usrename and password are correct then proceed to generate token
    //     if (user.EnName != "khadeja" || user.Password != "123")
    //     {
    //         return Unauthorized();
    //     }

    //     var token = Jwt.Generate(user, _config);
        
    //     return Ok(token);
    // }

    [HttpPost("authenticate/{token}")]
    public IActionResult Authenticate(string token)
    {
        var userId = Jwt.ValidateToken(token, _config);
        if (userId == null)
        {
            return Unauthorized();
        }

        return Ok(userId);
    }
}