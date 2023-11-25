
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRM.Models;
using CRM.Data;
using CRM.ViewModels;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CRM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DbAppContext _context;
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthenticationController(UserManager<User> userManager, 
    RoleManager<IdentityRole> roleManager, 
    DbAppContext context, 
    IConfiguration configuration, 
    TokenValidationParameters tokenValidationParameters)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _configuration = configuration;
        _tokenValidationParameters = tokenValidationParameters;
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
            var token = await GenerateJWTToken(user, null);
            return Ok(token);
        }

        return Unauthorized();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM model){
        if (!ModelState.IsValid)
            return BadRequest("Please, provide all the required fields.");

        var result = await VerifyAndGenerateToken(model);

        return Ok(result);
    }

    private async Task<AuthResultVM> GenerateJWTToken(User user, RefreshTokens rToken){
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

        if(rToken != null){
            var rTokenResponse = new AuthResultVM
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = rToken.Token,
                ExpiresAt = token.ValidTo
            };
            return rTokenResponse;
        }

        var refreshToken = new RefreshTokens
        {
            JwtId = token.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            IsRevoked = false,
            Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
        };
        
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResultVM
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            ExpiresAt = token.ValidTo
        };
    }

    private async Task<AuthResultVM> VerifyAndGenerateToken(TokenRequestVM model){
        
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

        var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);

        try{
            var tokenCheckResult = jwtTokenHandler.ValidateToken(model.Token, _tokenValidationParameters, out var validatedToken);

            return await GenerateJWTToken(dbUser, storedToken);
        }
        catch(SecurityTokenExpiredException){
            if (storedToken.ExpiresAt >= DateTime.UtcNow)
                return await GenerateJWTToken(dbUser, storedToken);
            else
                return await GenerateJWTToken(dbUser, null);
        }
    }
}