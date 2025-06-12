namespace ReviewGenie.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string FullName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
    public ICollection<Business> Businesses { get; } = new List<Business>();

    public static User Create(string name, string email)
        => new() { FullName = name, Email = email.ToLowerInvariant() };

    public void SetPasswordHash(string hash) => PasswordHash = hash;
}
