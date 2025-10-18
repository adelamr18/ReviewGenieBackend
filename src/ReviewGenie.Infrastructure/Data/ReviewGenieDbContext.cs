using Microsoft.EntityFrameworkCore;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Domain.ValueObjects;

namespace ReviewGenie.Infrastructure.Data;

public class ReviewGenieDbContext : DbContext
{
    public ReviewGenieDbContext(DbContextOptions opts) : base(opts) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<PlatformLink> Platforms => Set<PlatformLink>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewMetrics> ReviewMetrics => Set<ReviewMetrics>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<RefreshToken>().HasIndex(r => new { r.UserId, r.Token }).IsUnique();

        b.Entity<Business>().OwnsOne(biz => biz.Address);
        b.Entity<Business>().HasIndex(x => new { x.OwnerId, x.Name });
        b.Entity<Business>()
            .HasOne<User>()
            .WithMany(u => u.Businesses)
            .HasForeignKey(bu => bu.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<PlatformLink>()
            .HasOne<Business>()
            .WithMany(bu => bu.Platforms)
            .HasForeignKey(pl => pl.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<PlatformLink>().HasIndex(pl => new { pl.BusinessId, pl.Platform }).IsUnique();

        // Review entity configuration
        b.Entity<Review>().HasIndex(r => new { r.Platform, r.ExternalId }).IsUnique();
        b.Entity<Review>().HasIndex(r => r.BusinessId);
        b.Entity<Review>().HasIndex(r => r.PostedAt);
        b.Entity<Review>().HasIndex(r => r.Sentiment);

        // ReviewMetrics entity configuration
        b.Entity<ReviewMetrics>().HasIndex(rm => new { rm.BusinessId, rm.Date }).IsUnique();
        b.Entity<ReviewMetrics>().HasIndex(rm => rm.Date);

        base.OnModelCreating(b);
    }
}
