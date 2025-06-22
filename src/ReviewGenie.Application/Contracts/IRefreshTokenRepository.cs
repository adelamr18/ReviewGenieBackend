using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token);
    Task<RefreshToken?> GetAsync(Guid userId, string token);
    Task SaveChangesAsync();
}
