using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IGoogleBusinessService
{
    Task<string> GetAuthorizationUrlAsync(Guid businessId, string redirectUri);
    Task<BusinessIntegration> ExchangeCodeForTokensAsync(string code, Guid businessId, string redirectUri);
    Task<BusinessIntegration> RefreshTokenAsync(BusinessIntegration integration);
    Task<List<Review>> SyncReviewsAsync(BusinessIntegration integration);
    Task<bool> PostReviewReplyAsync(BusinessIntegration integration, string reviewId, string responseText);
    Task<GoogleBusinessProfile?> GetBusinessProfileAsync(BusinessIntegration integration);
}

public class GoogleBusinessProfile
{
    public string AccountId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
}
