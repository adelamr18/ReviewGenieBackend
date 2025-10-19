using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;

namespace ReviewGenie.Infrastructure.Services;

public class ExternalPlatformService : IExternalPlatformService
{
    private readonly IGoogleBusinessService _googleService;
    private readonly IBusinessIntegrationRepository _integrationRepo;

    public ExternalPlatformService(
        IGoogleBusinessService googleService,
        IBusinessIntegrationRepository integrationRepo)
    {
        _googleService = googleService;
        _integrationRepo = integrationRepo;
    }

    public async Task<List<CreateReviewDto>> SyncReviewsAsync(Guid businessId)
    {
        var allReviews = new List<CreateReviewDto>();

        var googleIntegration = await _integrationRepo.GetByBusinessAndPlatformAsync(businessId, "google");
        if (googleIntegration != null)
        {
            try
            {
                var googleReviews = await _googleService.SyncReviewsAsync(googleIntegration);
                allReviews.AddRange(googleReviews.Select(MapToCreateDto));
                
                googleIntegration.LastSyncAt = DateTime.UtcNow;
                await _integrationRepo.UpdateAsync(googleIntegration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to sync Google reviews: {ex.Message}");
            }
        }

        return allReviews;
    }

    public async Task<bool> PostResponseAsync(Guid businessId, string reviewExternalId, string response)
    {
        var googleIntegration = await _integrationRepo.GetByBusinessAndPlatformAsync(businessId, "google");
        if (googleIntegration != null)
        {
            try
            {
                return await _googleService.PostReviewReplyAsync(googleIntegration, reviewExternalId, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to post Google response: {ex.Message}");
                return false;
            }
        }

        return false;
    }

    public async Task<bool> ValidateCredentialsAsync(Guid businessId)
    {
        var integrations = await _integrationRepo.GetByBusinessIdAsync(businessId);
        return integrations.Any(i => i.IsActive && DateTime.UtcNow < i.ExpiresAt);
    }

    private static CreateReviewDto MapToCreateDto(Domain.Entities.Review review)
    {
        return new CreateReviewDto(
            review.BusinessId,
            review.Platform,
            review.ExternalId,
            review.AuthorName,
            review.AuthorEmail,
            review.Rating,
            review.Text,
            review.PostedAt,
            review.PlatformUrl,
            review.AuthorPhotoUrl,
            review.IsVerified
        );
    }
}
