using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IJwtFactory
{
    string GenerateJwt(User user);
    string GenerateRefreshToken();
}
