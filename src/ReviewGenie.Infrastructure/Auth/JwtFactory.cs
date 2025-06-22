using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Infrastructure.Auth;

public class JwtFactory : IJwtFactory
{
    private readonly JwtSettings _cfg;
    public JwtFactory(IOptions<JwtSettings> opt) => _cfg = opt.Value;

    public string GenerateJwt(User u)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, u.Email),
            new Claim("name", u.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _cfg.Issuer,
            _cfg.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(_cfg.ExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
