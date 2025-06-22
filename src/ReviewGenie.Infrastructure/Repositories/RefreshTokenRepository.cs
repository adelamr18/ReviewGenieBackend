using Microsoft.EntityFrameworkCore;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ReviewGenieDbContext _db;
    public RefreshTokenRepository(ReviewGenieDbContext db) => _db = db;

    public Task AddAsync(RefreshToken rt)          { _db.RefreshTokens.Add(rt); return Task.CompletedTask; }
    public Task<RefreshToken?> GetAsync(Guid uid, string token)
        => _db.RefreshTokens.FirstOrDefaultAsync(r => r.UserId == uid && r.Token == token);
    public Task SaveChangesAsync()                 => _db.SaveChangesAsync();
}
