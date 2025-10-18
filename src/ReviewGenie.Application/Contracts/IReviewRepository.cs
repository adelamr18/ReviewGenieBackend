using ReviewGenie.Application.Dto;
using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Contracts;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id);
    Task<Review?> GetByExternalIdAsync(string platform, string externalId);
    Task<ReviewListDto> GetReviewsAsync(ReviewFiltersDto filters);
    Task<List<Review>> GetReviewsByBusinessIdAsync(Guid businessId);
    Task<List<Review>> GetUnrespondedReviewsAsync(Guid businessId);
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string platform, string externalId);
    Task<int> GetReviewCountAsync(Guid businessId);
    Task<double> GetAverageRatingAsync(Guid businessId);
    Task<ReviewAnalyticsDto> GetAnalyticsAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ReviewMetrics>> GetMetricsAsync(Guid businessId, DateTime fromDate, DateTime toDate);
    Task<ReviewMetrics?> GetMetricsForDateAsync(Guid businessId, DateTime date);
    Task<ReviewMetrics> CreateMetricsAsync(ReviewMetrics metrics);
    Task<ReviewMetrics> UpdateMetricsAsync(ReviewMetrics metrics);
}
