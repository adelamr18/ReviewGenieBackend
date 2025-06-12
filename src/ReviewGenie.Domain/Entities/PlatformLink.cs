namespace ReviewGenie.Domain.Entities;

public enum PlatformType { Google = 1, Yelp = 2 }

public class PlatformLink
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BusinessId { get; init; }        // FK → Business
    public PlatformType Platform { get; init; }
    public string Url { get; init; } = null!;
    public bool Verified { get; private set; }

    public static PlatformLink Create(Guid bizId, PlatformType p, string url)
        => new() { BusinessId = bizId, Platform = p, Url = url.Trim() };

    public void MarkVerified() => Verified = true;
}
