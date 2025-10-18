using Microsoft.EntityFrameworkCore;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Infrastructure.Data;

namespace ReviewGenie.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ReviewGenieDbContext _context;

    public ReviewRepository(ReviewGenieDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        return await _context.Reviews
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Review?> GetByExternalIdAsync(string platform, string externalId)
    {
        return await _context.Reviews
            .Include(r => r.Business)
            .FirstOrDefaultAsync(r => r.Platform == platform && r.ExternalId == externalId);
    }

    public async Task<ReviewListDto> GetReviewsAsync(ReviewFiltersDto filters)
    {
        var query = _context.Reviews
            .Include(r => r.Business)
            .AsQueryable();

        // Apply filters
        if (filters.BusinessId.HasValue)
            query = query.Where(r => r.BusinessId == filters.BusinessId.Value);

        if (!string.IsNullOrEmpty(filters.Platform))
            query = query.Where(r => r.Platform == filters.Platform);

        if (!string.IsNullOrEmpty(filters.Sentiment))
            query = query.Where(r => r.Sentiment == filters.Sentiment);

        if (filters.HasResponded.HasValue)
            query = query.Where(r => r.HasResponded == filters.HasResponded.Value);

        if (filters.MinRating.HasValue)
            query = query.Where(r => r.Rating >= filters.MinRating.Value);

        if (filters.MaxRating.HasValue)
            query = query.Where(r => r.Rating <= filters.MaxRating.Value);

        if (filters.FromDate.HasValue)
            query = query.Where(r => r.PostedAt >= filters.FromDate.Value);

        if (filters.ToDate.HasValue)
            query = query.Where(r => r.PostedAt <= filters.ToDate.Value);

        if (!string.IsNullOrEmpty(filters.SearchTerm))
        {
            var searchTerm = filters.SearchTerm.ToLower();
            query = query.Where(r => 
                r.Text.ToLower().Contains(searchTerm) || 
                r.AuthorName.ToLower().Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var reviews = await query
            .OrderByDescending(r => r.PostedAt)
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(r => new ReviewDto(
                r.Id,
                r.BusinessId,
                r.Platform,
                r.ExternalId,
                r.AuthorName,
                r.AuthorEmail,
                r.Rating,
                r.Text,
                r.PostedAt,
                r.CreatedAt,
                r.Sentiment,
                r.GeneratedResponse,
                r.HasResponded,
                r.RespondedAt,
                r.ResponseText,
                r.PlatformUrl,
                r.AuthorPhotoUrl,
                r.IsVerified
            ))
            .ToListAsync();

        return new ReviewListDto(reviews, totalCount, filters.PageNumber, filters.PageSize);
    }

    public async Task<List<Review>> GetReviewsByBusinessIdAsync(Guid businessId)
    {
        return await _context.Reviews
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.PostedAt)
            .ToListAsync();
    }

    public async Task<List<Review>> GetUnrespondedReviewsAsync(Guid businessId)
    {
        return await _context.Reviews
            .Where(r => r.BusinessId == businessId && !r.HasResponded)
            .OrderByDescending(r => r.PostedAt)
            .ToListAsync();
    }

    public async Task<Review> CreateAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task DeleteAsync(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string platform, string externalId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.Platform == platform && r.ExternalId == externalId);
    }

    public async Task<int> GetReviewCountAsync(Guid businessId)
    {
        return await _context.Reviews
            .CountAsync(r => r.BusinessId == businessId);
    }

    public async Task<double> GetAverageRatingAsync(Guid businessId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.BusinessId == businessId)
            .ToListAsync();

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<ReviewAnalyticsDto> GetAnalyticsAsync(Guid businessId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Reviews.Where(r => r.BusinessId == businessId);

        if (fromDate.HasValue)
            query = query.Where(r => r.PostedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.PostedAt <= toDate.Value);

        var reviews = await query.ToListAsync();

        var totalReviews = reviews.Count;
        var respondedReviews = reviews.Count(r => r.HasResponded);
        var responseRate = totalReviews > 0 ? (double)respondedReviews / totalReviews * 100 : 0;
        var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        var positiveReviews = reviews.Count(r => r.Sentiment == "positive");
        var neutralReviews = reviews.Count(r => r.Sentiment == "neutral");
        var negativeReviews = reviews.Count(r => r.Sentiment == "negative");

        // Get daily metrics for the last 30 days
        var endDate = toDate ?? DateTime.UtcNow;
        var startDate = fromDate ?? endDate.AddDays(-30);
        var dailyMetrics = await GetMetricsAsync(businessId, startDate, endDate);

        return new ReviewAnalyticsDto(
            totalReviews,
            respondedReviews,
            responseRate,
            averageRating,
            positiveReviews,
            neutralReviews,
            negativeReviews,
            dailyMetrics.Select(m => new ReviewMetricsDto(
                m.Date,
                m.TotalReviews,
                m.PositiveReviews,
                m.NeutralReviews,
                m.NegativeReviews,
                m.RespondedReviews,
                m.AverageRating,
                m.NewReviews
            )).ToList()
        );
    }

    public async Task<List<ReviewMetrics>> GetMetricsAsync(Guid businessId, DateTime fromDate, DateTime toDate)
    {
        return await _context.ReviewMetrics
            .Where(m => m.BusinessId == businessId && m.Date >= fromDate && m.Date <= toDate)
            .OrderBy(m => m.Date)
            .ToListAsync();
    }

    public async Task<ReviewMetrics?> GetMetricsForDateAsync(Guid businessId, DateTime date)
    {
        return await _context.ReviewMetrics
            .FirstOrDefaultAsync(m => m.BusinessId == businessId && m.Date.Date == date.Date);
    }

    public async Task<ReviewMetrics> CreateMetricsAsync(ReviewMetrics metrics)
    {
        _context.ReviewMetrics.Add(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<ReviewMetrics> UpdateMetricsAsync(ReviewMetrics metrics)
    {
        _context.ReviewMetrics.Update(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }
}
