namespace ReviewGenie.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Token { get; init; } = null!;
    public Guid UserId { get; init; }          // FK
    public DateTime ExpiresAt { get; init; }
    public bool Revoked { get; private set; }

    public void Revoke() => Revoked = true;
}
