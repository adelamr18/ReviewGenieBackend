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
        return Ok(new { token = jwt, refresh });
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
}
