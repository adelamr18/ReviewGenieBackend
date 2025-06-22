namespace ReviewGenie.Application.Contracts;

public interface IAuthService
{
    Task<string> RegisterAsync(string name, string email, string password, string confirm);
    Task<(string jwt, string refresh)> LoginAsync(string email, string password);
    Task LogoutAsync(Guid userId, string refreshToken);
}
