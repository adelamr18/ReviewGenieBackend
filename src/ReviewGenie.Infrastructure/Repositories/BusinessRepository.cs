using Microsoft.EntityFrameworkCore;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly ReviewGenieDbContext _db;
    public BusinessRepository(ReviewGenieDbContext db) => _db = db;

    public Task AddAsync(Business b)                 { _db.Businesses.Add(b); return Task.CompletedTask; }
    public Task<Business?> GetAsync(Guid id)         => _db.Businesses
                                                           .Include(b => b.Platforms)
                                                           .FirstOrDefaultAsync(b => b.Id == id);
    public Task SaveChangesAsync()                   => _db.SaveChangesAsync();
}
