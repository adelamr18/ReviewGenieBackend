using Microsoft.AspNetCore.Identity;          // ← add this
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRefreshTokenRepository _tokens;
        private readonly IPasswordHasher<User> _hasher;
        private readonly IJwtFactory _jwt;

        public AuthService(IUserRepository u,
                           IRefreshTokenRepository t,
                           IPasswordHasher<User> hasher,
                           IJwtFactory jwt)
            => (_users, _tokens, _hasher, _jwt) = (u, t, hasher, jwt);

        public async Task<string> RegisterAsync(string name, string email,
                                                string pwd, string confirm)
        {
            if (pwd != confirm) throw new ArgumentException("Password mismatch");
            if (await _users.GetByEmailAsync(email) is not null)
                throw new InvalidOperationException("Email exists");

            var user = User.Create(name, email);
            user.SetPasswordHash(_hasher.HashPassword(user, pwd));

            await _users.AddAsync(user);
            await _users.SaveChangesAsync();

            return _jwt.GenerateJwt(user);
        }

        public async Task<(string jwt, string refresh)> LoginAsync(string email, string pwd)
        {
            var user = await _users.GetByEmailAsync(email)
                       ?? throw new UnauthorizedAccessException("Bad creds");

            var vr = _hasher.VerifyHashedPassword(user, user.PasswordHash, pwd);
            if (vr == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("Bad creds");

            var jwt = _jwt.GenerateJwt(user);
            var refresh = _jwt.GenerateRefreshToken();

            await _tokens.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refresh,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await _tokens.SaveChangesAsync();
            return (jwt, refresh);
        }

        public async Task LogoutAsync(Guid uid, string refresh)
        {
            var rt = await _tokens.GetAsync(uid, refresh);
            if (rt is null) return;
            rt.Revoke();
            await _tokens.SaveChangesAsync();
        }
    }
}
