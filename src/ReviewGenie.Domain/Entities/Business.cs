using ReviewGenie.Domain.ValueObjects;

namespace ReviewGenie.Domain.Entities;

public class Business
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OwnerId { get; init; }               // FK → User

    public string Name { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public string Phone { get; private set; } = null!;
    public string? Website { get; private set; }
    public string Description { get; private set; } = null!;

    public ICollection<PlatformLink> Platforms { get; } = new List<PlatformLink>();
    public ICollection<Review> Reviews { get; } = new List<Review>();
    public ICollection<ReviewMetrics> ReviewMetrics { get; } = new List<ReviewMetrics>();

    public static Business Create(Guid ownerId, string name, string type,
                                  Address address, string phone,
                                  string? website, string desc)
        => new()
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Type = type.Trim(),
            Address = address,
            Phone = phone.Trim(),
            Website = website?.Trim(),
            Description = desc.Trim()
        };
}
