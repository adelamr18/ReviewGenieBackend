using ReviewGenie.Application.DTO;

namespace ReviewGenie.Application.Contracts;

public interface IOnboardingService
{
    Task<Guid> AddBusinessAsync(Guid ownerId, BusinessDto dto);
    Task AddPlatformsAsync(Guid ownerId, Guid businessId, PlatformDto dto);
    Task<BusinessDto?> GetBusinessAsync(Guid ownerId, Guid businessId);
}

