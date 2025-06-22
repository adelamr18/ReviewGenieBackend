using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
