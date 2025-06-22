using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IPlatformLinkRepository
{
    Task AddRangeAsync(IEnumerable<PlatformLink> links);
    Task SaveChangesAsync();
}
