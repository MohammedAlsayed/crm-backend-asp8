
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRM.Models;
using CRM.Data;
using CRM.ViewModels;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DbAppContext _context;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, DbAppContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterVM model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Please, provide all the required fields.");

        var userExists = await _userManager.FindByEmailAsync(model.Email);
        if (userExists != null)
            return BadRequest($"User {model.Email} already exists!");

        User newUser = new User()
        {   
            EnName = model.EnName,
            ArName = model.ArName,
            Email = model.Email,
            UserName = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
        };

        var result = await _userManager.CreateAsync(newUser, model.Password);
        // print errors
        foreach (var error in result.Errors)
            Console.WriteLine(error);

        if (!result.Succeeded)
            return BadRequest($"User {model.Email} creation failed!");

        return Ok("User created successfully!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginVM model)
    {
        if (!ModelState.IsValid)
            return BadRequest("Please, provide all the required fields.");

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password)){
            var token = await GenerateJWTToken(user);
            return Ok(token);
        }

        return Unauthorized();
    }

    private async Task<AuthResultVM> GenerateJWTToken(User user){
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // var userRoles = await _userManager.GetRolesAsync(user);
        // foreach (var userRole in userRoles)
        // {
        //     authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        //     var role = await _roleManager.FindByNameAsync(userRole);
        //     if (role != null)
        //     {
        //         var roleClaims = await _roleManager.GetClaimsAsync(role);
        //         foreach (Claim roleClaim in roleClaims)
        //         {
        //             authClaims.Add(roleClaim);
        //         }
        //     }
        // }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResultVM
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = token.ValidTo
        };
    }
}