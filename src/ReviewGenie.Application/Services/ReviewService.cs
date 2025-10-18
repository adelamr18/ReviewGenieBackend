using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;
using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAiService _aiService;
    private readonly IExternalPlatformService _platformService;

    public ReviewService(IReviewRepository reviewRepository, IAiService aiService, IExternalPlatformService platformService)
    {
        _reviewRepository = reviewRepository;
        _aiService = aiService;
        _platformService = platformService;
    }

    public async Task<ReviewDto?> GetReviewAsync(Guid id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        return review != null ? MapToDto(review) : null;
    }

    public async Task<ReviewListDto> GetReviewsAsync(ReviewFiltersDto filters)
    {
        return await _reviewRepository.GetReviewsAsync(filters);
    }

    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto)
    {
        // Check if review already exists
        if (await _reviewRepository.ExistsAsync(dto.Platform, dto.ExternalId))
        {
            var existing = await _reviewRepository.GetByExternalIdAsync(dto.Platform, dto.ExternalId);
            if (existing != null)
                return MapToDto(existing);
        }

        var review = Review.Create(
            dto.BusinessId,
            dto.Platform,
            dto.ExternalId,
            dto.AuthorName,
            dto.AuthorEmail,
            dto.Rating,
            dto.Text,
            dto.PostedAt,
            dto.PlatformUrl,
            dto.AuthorPhotoUrl,
            dto.IsVerified
        );

        var created = await _reviewRepository.CreateAsync(review);
        return MapToDto(created);
    }

    public async Task<ReviewDto> UpdateReviewAsync(Guid id, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new ArgumentException("Review not found");

        if (dto.Sentiment != null)
            review.SetSentiment(dto.Sentiment);

        if (dto.GeneratedResponse != null)
            review.SetGeneratedResponse(dto.GeneratedResponse);

        if (dto.HasResponded == true && dto.ResponseText != null)
            review.MarkAsResponded(dto.ResponseText);

        var updated = await _reviewRepository.UpdateAsync(review);
        return MapToDto(updated);
    }

    public async Task DeleteReviewAsync(Guid id)
    {
        await _reviewRepository.DeleteAsync(id);
    }

    public async Task<ReviewDto> GenerateResponseAsync(GenerateResponseDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);
        if (review == null)
            throw new ArgumentException("Review not found");

        var (sentiment, response) = await _aiService.AnalyzeAndGenerateResponseAsync(dto);
        
        review.SetSentiment(sentiment);
        review.SetGeneratedResponse(response);

        var updated = await _reviewRepository.UpdateAsync(review);
        return MapToDto(updated);
    }

    public async Task<ReviewDto> ApproveResponseAsync(Guid reviewId, string? customResponse = null)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            throw new ArgumentException("Review not found");

        var responseText = customResponse ?? review.GeneratedResponse ?? throw new InvalidOperationException("No response to approve");
        review.MarkAsResponded(responseText);

        // Post response to external platform
        try
        {
            await _platformService.PostResponseAsync(review.BusinessId, review.ExternalId, responseText);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the operation
            // In a real implementation, you'd have proper logging
            Console.WriteLine($"Failed to post response to {review.Platform}: {ex.Message}");
        }

        var updated = await _reviewRepository.UpdateAsync(review);
        return MapToDto(updated);
    }

    public async Task<ReviewAnalyticsDto> GetAnalyticsAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await _reviewRepository.GetAnalyticsAsync(businessId, fromDate, toDate);
    }

    public async Task<List<ReviewDto>> SyncReviewsAsync(Guid businessId)
    {
        var newReviews = await _platformService.SyncReviewsAsync(businessId);
        var createdReviews = new List<ReviewDto>();

        foreach (var reviewDto in newReviews)
        {
            var review = await CreateReviewAsync(reviewDto);
            createdReviews.Add(review);
        }

        return createdReviews;
    }

    public async Task<ReviewMetricsDto> CalculateDailyMetricsAsync(Guid businessId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var reviews = await _reviewRepository.GetReviewsAsync(new ReviewFiltersDto
        {
            BusinessId = businessId,
            FromDate = startOfDay,
            ToDate = endOfDay
        });

        var allReviews = reviews.Reviews;
        var totalReviews = allReviews.Count;
        var positiveReviews = allReviews.Count(r => r.Sentiment == "positive");
        var neutralReviews = allReviews.Count(r => r.Sentiment == "neutral");
        var negativeReviews = allReviews.Count(r => r.Sentiment == "negative");
        var respondedReviews = allReviews.Count(r => r.HasResponded);
        var averageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;

        // Get previous day's total to calculate new reviews
        var previousDay = startOfDay.AddDays(-1);
        var previousDayReviews = await _reviewRepository.GetReviewsAsync(new ReviewFiltersDto
        {
            BusinessId = businessId,
            FromDate = previousDay,
            ToDate = startOfDay
        });
        var newReviews = totalReviews - previousDayReviews.TotalCount;

        var metrics = ReviewMetrics.Create(
            businessId,
            startOfDay,
            totalReviews,
            positiveReviews,
            neutralReviews,
            negativeReviews,
            respondedReviews,
            averageRating,
            newReviews
        );

        var existingMetrics = await _reviewRepository.GetMetricsForDateAsync(businessId, startOfDay);
        if (existingMetrics != null)
        {
            // Update existing metrics
            await _reviewRepository.UpdateMetricsAsync(metrics);
        }
        else
        {
            // Create new metrics
            await _reviewRepository.CreateMetricsAsync(metrics);
        }

        return new ReviewMetricsDto(
            startOfDay,
            totalReviews,
            positiveReviews,
            neutralReviews,
            negativeReviews,
            respondedReviews,
            averageRating,
            newReviews
        );
    }

    private static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto(
            review.Id,
            review.BusinessId,
            review.Platform,
            review.ExternalId,
            review.AuthorName,
            review.AuthorEmail,
            review.Rating,
            review.Text,
            review.PostedAt,
            review.CreatedAt,
            review.Sentiment,
            review.GeneratedResponse,
            review.HasResponded,
            review.RespondedAt,
            review.ResponseText,
            review.PlatformUrl,
            review.AuthorPhotoUrl,
            review.IsVerified
        );
    }
}
