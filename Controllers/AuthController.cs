
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
public class AuthController: ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DbAppContext _context;
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public AuthController(UserManager<User> userManager, 
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

        if (!result.Succeeded)
            return BadRequest($"User {model.Email} creation failed!");

        // Add User Role
        if (await _roleManager.RoleExistsAsync(model.Role)){
            await _userManager.AddToRoleAsync(newUser, model.Role);
        }
        
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

    [HttpPost("refresh_token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM model){
        if (!ModelState.IsValid)
            return BadRequest("Please, provide all the required fields.");

        try{
            var result = await VerifyAndGenerateToken(model);
            return Ok(result);
        }
        catch(Exception ex){
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutVM model){
        Console.WriteLine("LOGOUT CALLED");
        if (!ModelState.IsValid) return BadRequest("Please, provide all the required fields.");

        // var rToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.RefreshToken);
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == model.userId);
        _context.RefreshTokens.Where(x => x.UserId == model.userId).ToList().ForEach(x => _context.RefreshTokens.Remove(x));
        await _context.SaveChangesAsync();

        return Ok("Logout successful");
    }

    private async Task<AuthResultVM> GenerateJWTToken(User user, RefreshTokens rToken){
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };


        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            expires: DateTime.Now.AddMinutes(10),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        ); 

        if(rToken != null){
            var rTokenResponse = new AuthResultVM
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = rToken.Token,
                ExpiresAt = token.ValidTo,
                UserId = user.Id
            };
            return rTokenResponse;
        }

        var refreshToken = new RefreshTokens
        {
            JwtId = token.Id,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
        };
        
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResultVM
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            ExpiresAt = token.ValidTo,
            UserId = user.Id
        };
    }

    private async Task<AuthResultVM> VerifyAndGenerateToken(TokenRequestVM model){
        
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == model.RefreshToken);

        if (storedToken == null){
            throw new SecurityTokenException("Invalid refresh token");
        }
    
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