using ReviewGenie.Application.Dto;

namespace ReviewGenie.Application.Contracts;

public interface IReviewService
{
    Task<ReviewDto?> GetReviewAsync(Guid id);
    Task<ReviewListDto> GetReviewsAsync(ReviewFiltersDto filters);
    Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto);
    Task<ReviewDto> UpdateReviewAsync(Guid id, UpdateReviewDto dto);
    Task DeleteReviewAsync(Guid id);
    Task<ReviewDto> GenerateResponseAsync(GenerateResponseDto dto);
    Task<ReviewDto> ApproveResponseAsync(Guid reviewId, string? customResponse = null);
    Task<ReviewAnalyticsDto> GetAnalyticsAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ReviewDto>> SyncReviewsAsync(Guid businessId);
    Task<ReviewMetricsDto> CalculateDailyMetricsAsync(Guid businessId, DateTime date);
}

