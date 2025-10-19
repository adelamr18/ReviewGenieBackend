using Microsoft.EntityFrameworkCore;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class BusinessIntegrationRepository : IBusinessIntegrationRepository
{
    private readonly ReviewGenieDbContext _context;

    public BusinessIntegrationRepository(ReviewGenieDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessIntegration?> GetByBusinessAndPlatformAsync(Guid businessId, string platform)
    {
        return await _context.BusinessIntegrations
            .FirstOrDefaultAsync(bi => bi.BusinessId == businessId && bi.Platform == platform && bi.IsActive);
    }

    public async Task<BusinessIntegration?> GetByIdAsync(Guid id)
    {
        return await _context.BusinessIntegrations.FindAsync(id);
    }

    public async Task<List<BusinessIntegration>> GetByBusinessIdAsync(Guid businessId)
    {
        return await _context.BusinessIntegrations
            .Where(bi => bi.BusinessId == businessId && bi.IsActive)
            .ToListAsync();
    }

    public async Task<BusinessIntegration> CreateAsync(BusinessIntegration integration)
    {
        integration.Id = Guid.NewGuid();
        integration.ConnectedAt = DateTime.UtcNow;
        
        _context.BusinessIntegrations.Add(integration);
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task<BusinessIntegration> UpdateAsync(BusinessIntegration integration)
    {
        _context.BusinessIntegrations.Update(integration);
        await _context.SaveChangesAsync();
        return integration;
    }

    public async Task DeleteAsync(Guid id)
    {
        var integration = await GetByIdAsync(id);
        if (integration != null)
        {
            integration.IsActive = false;
            await UpdateAsync(integration);
        }
    }
}
