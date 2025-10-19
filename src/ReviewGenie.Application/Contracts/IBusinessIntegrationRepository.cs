using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IBusinessIntegrationRepository
{
    Task<BusinessIntegration?> GetByBusinessAndPlatformAsync(Guid businessId, string platform);
    Task<BusinessIntegration?> GetByIdAsync(Guid id);
    Task<List<BusinessIntegration>> GetByBusinessIdAsync(Guid businessId);
    Task<BusinessIntegration> CreateAsync(BusinessIntegration integration);
    Task<BusinessIntegration> UpdateAsync(BusinessIntegration integration);
    Task DeleteAsync(Guid id);
}
