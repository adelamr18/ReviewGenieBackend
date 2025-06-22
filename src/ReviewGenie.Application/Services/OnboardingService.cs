using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.DTO;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Domain.ValueObjects;

namespace ReviewGenie.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IBusinessRepository _bizRepo;
    private readonly IPlatformLinkRepository _platRepo;

    public OnboardingService(IBusinessRepository b, IPlatformLinkRepository p)
        => (_bizRepo, _platRepo) = (b, p);

    public async Task<Guid> AddBusinessAsync(Guid ownerId, BusinessDto d)
    {
        var addr = new Address(d.Street, d.City, d.State, d.Zip);
        var biz = Business.Create(ownerId, d.Name, d.Type, addr,
                                   d.Phone, d.Website, d.Description);

        await _bizRepo.AddAsync(biz);
        await _bizRepo.SaveChangesAsync();
        return biz.Id;
    }

    public async Task AddPlatformsAsync(Guid ownerId, Guid businessId, PlatformDto dto)
    {
        var biz = await _bizRepo.GetAsync(businessId)
                  ?? throw new KeyNotFoundException("Business not found");

        if (biz.OwnerId != ownerId)
            throw new UnauthorizedAccessException("Not your business");

        var links = new List<PlatformLink>
    {
        PlatformLink.Create(businessId, PlatformType.Google, dto.GoogleUrl)
    };
        if (!string.IsNullOrWhiteSpace(dto.YelpUrl))
            links.Add(PlatformLink.Create(businessId, PlatformType.Yelp, dto.YelpUrl));

        await _platRepo.AddRangeAsync(links);
        await _platRepo.SaveChangesAsync();
    }

    public async Task<BusinessDto?> GetBusinessAsync(Guid ownerId, Guid businessId)
    {
        var biz = await _bizRepo.GetAsync(businessId);
        if (biz is null || biz.OwnerId != ownerId) return null;

        return new BusinessDto(
            biz.Name,
            biz.Type,
            biz.Address.Street,
            biz.Address.City,
            biz.Address.State,
            biz.Address.Zip,
            biz.Phone,
            biz.Website,
            biz.Description);
    }


}
