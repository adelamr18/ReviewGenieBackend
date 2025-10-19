using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace ReviewGenie.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService a) => _auth = a;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var token = await _auth.RegisterAsync(dto.FullName, dto.Email, dto.Password, dto.ConfirmPassword);

        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var (jwt, refresh) = await _auth.LoginAsync(dto.Email, dto.Password);

        // Parse JWT to extract user info
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(jwt);
        
        Console.WriteLine("JWT Claims:");
        foreach (var claim in jsonToken.Claims)
        {
            Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
        }
        
        var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value 
                    ?? jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
        var email = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value 
                   ?? jsonToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
        var name = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value 
                  ?? jsonToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
        
        // Split name into first and last name (simple approach)
        var nameParts = name?.Split(' ', 2) ?? new[] { "", "" };
        var firstName = nameParts.Length > 0 ? nameParts[0] : "";
        var lastName = nameParts.Length > 1 ? nameParts[1] : "";

        var user = new
        {
            id = userId,
            email = email,
            firstName = firstName,
            lastName = lastName
        };

        return Ok(new { token = jwt, refresh, user });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromHeader(Name = "X-Refresh-Token")] string refresh)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await _auth.LogoutAsync(uid, refresh);

        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(ClaimTypes.Email)
                   ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name)
                      ?? User.FindFirstValue("name");

        if (id is null)
            return Unauthorized();

        return Ok(new
        {
            id,
            fullName = fullName ?? string.Empty,
            email = email ?? string.Empty
        });
    }
}

