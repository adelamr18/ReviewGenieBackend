using Microsoft.Extensions.Logging;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;

namespace ReviewGenie.Infrastructure.Services;

public class MockExternalPlatformService : IExternalPlatformService
{
    private readonly ILogger<MockExternalPlatformService> _logger;

    public MockExternalPlatformService(ILogger<MockExternalPlatformService> logger)
    {
        _logger = logger;
    }

    public async Task<List<CreateReviewDto>> SyncReviewsAsync(Guid businessId)
    {
        // Simulate API delay
        await Task.Delay(1000);

        // Return mock reviews for testing
        var mockReviews = new List<CreateReviewDto>
        {
            new CreateReviewDto(
                businessId,
                "Google",
                "google_review_1",
                "John Smith",
                "john.smith@email.com",
                5,
                "Amazing service! The staff was incredibly helpful and the food was outstanding. Will definitely be back!",
                DateTime.UtcNow.AddDays(-1),
                "https://maps.google.com/review1",
                "https://lh3.googleusercontent.com/avatar1.jpg",
                true
            ),
            new CreateReviewDto(
                businessId,
                "Google",
                "google_review_2",
                "Sarah Johnson",
                "sarah.johnson@email.com",
                4,
                "Great experience overall. The atmosphere was nice and the service was good. Food was delicious!",
                DateTime.UtcNow.AddDays(-2),
                "https://maps.google.com/review2",
                "https://lh3.googleusercontent.com/avatar2.jpg",
                true
            ),
            new CreateReviewDto(
                businessId,
                "Yelp",
                "yelp_review_1",
                "Mike Chen",
                "mike.chen@email.com",
                2,
                "The wait time was too long and the food was cold when it arrived. Not impressed with the service.",
                DateTime.UtcNow.AddDays(-3),
                "https://www.yelp.com/review1",
                "https://s3-media.yelpcdn.com/avatar1.jpg",
                false
            ),
            new CreateReviewDto(
                businessId,
                "Google",
                "google_review_3",
                "Emma Davis",
                "emma.davis@email.com",
                3,
                "Average experience. The food was okay but nothing special. Service was friendly though.",
                DateTime.UtcNow.AddDays(-4),
                "https://maps.google.com/review3",
                "https://lh3.googleusercontent.com/avatar3.jpg",
                true
            ),
            new CreateReviewDto(
                businessId,
                "Yelp",
                "yelp_review_2",
                "David Wilson",
                "david.wilson@email.com",
                5,
                "Excellent service! The team was professional and the results exceeded my expectations. Highly recommend!",
                DateTime.UtcNow.AddDays(-5),
                "https://www.yelp.com/review2",
                "https://s3-media.yelpcdn.com/avatar2.jpg",
                true
            )
        };

        _logger.LogInformation("Synced {Count} mock reviews for business {BusinessId}", mockReviews.Count, businessId);
        return mockReviews;
    }

    public async Task<bool> PostResponseAsync(Guid businessId, string reviewExternalId, string response)
    {
        // Simulate API delay
        await Task.Delay(500);

        _logger.LogInformation("Posted response to review {ReviewId} for business {BusinessId}: {Response}", 
            reviewExternalId, businessId, response);
        
        // Simulate success
        return true;
    }

    public async Task<bool> ValidateCredentialsAsync(Guid businessId)
    {
        // Simulate API delay
        await Task.Delay(200);

        _logger.LogInformation("Validated credentials for business {BusinessId}", businessId);
        
        // Simulate success
        return true;
    }
}
