using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IBusinessRepository
{
    Task AddAsync(Business biz);
    Task<Business?> GetAsync(Guid id);
    Task<List<Business>> GetAllAsync();
    Task<List<Business>> GetByOwnerIdAsync(Guid ownerId);
    Task SaveChangesAsync();
}
