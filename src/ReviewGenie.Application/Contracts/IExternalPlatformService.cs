using ReviewGenie.Application.Dto;

namespace ReviewGenie.Application.Contracts;

public interface IExternalPlatformService
{
    Task<List<CreateReviewDto>> SyncReviewsAsync(Guid businessId);
    Task<bool> PostResponseAsync(Guid businessId, string reviewExternalId, string response);
    Task<bool> ValidateCredentialsAsync(Guid businessId);
}

public interface IGoogleMyBusinessService : IExternalPlatformService
{
    Task<string> GetAccessTokenAsync(string refreshToken);
    Task<List<CreateReviewDto>> GetReviewsAsync(string accessToken, string locationId);
    Task<bool> PostResponseAsync(string accessToken, string reviewName, string response);
}

public interface IYelpService : IExternalPlatformService
{
    Task<List<CreateReviewDto>> GetReviewsAsync(string businessId);
    Task<bool> PostResponseAsync(string businessId, string reviewId, string response);
}

