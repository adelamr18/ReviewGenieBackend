using Microsoft.EntityFrameworkCore;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ReviewGenieDbContext _db;
    public UserRepository(ReviewGenieDbContext db) => _db = db;

    public Task AddAsync(User u)                   { _db.Users.Add(u); return Task.CompletedTask; }
    public Task<User?> GetByEmailAsync(string e)   => _db.Users
                                                         .Include(u => u.RefreshTokens)
                                                         .FirstOrDefaultAsync(u => u.Email == e.ToLower());
    public Task<User?> GetByIdAsync(Guid id)       => _db.Users.FindAsync(id).AsTask();
    public Task SaveChangesAsync()                 => _db.SaveChangesAsync();
}
