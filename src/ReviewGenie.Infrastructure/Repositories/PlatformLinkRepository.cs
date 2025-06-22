using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class PlatformLinkRepository : IPlatformLinkRepository
{
    private readonly ReviewGenieDbContext _db;
    public PlatformLinkRepository(ReviewGenieDbContext db) => _db = db;

    public Task AddRangeAsync(IEnumerable<PlatformLink> l) { _db.Platforms.AddRange(l); return Task.CompletedTask; }
    public Task SaveChangesAsync()                          => _db.SaveChangesAsync();
}
