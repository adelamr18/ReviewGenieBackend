namespace ReviewGenie.Domain.Entities;

public class ReviewMetrics
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BusinessId { get; init; }
    public Business Business { get; init; } = null!;
    
    public DateTime Date { get; init; }
    public int TotalReviews { get; init; }
    public int PositiveReviews { get; init; }
    public int NeutralReviews { get; init; }
    public int NegativeReviews { get; init; }
    public int RespondedReviews { get; init; }
    public double AverageRating { get; init; }
    public int NewReviews { get; init; }
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public static ReviewMetrics Create(Guid businessId, DateTime date, 
                                    int totalReviews, int positiveReviews, 
                                    int neutralReviews, int negativeReviews,
                                    int respondedReviews, double averageRating, 
                                    int newReviews)
    {
        return new ReviewMetrics
        {
            BusinessId = businessId,
            Date = date.Date, // Store only the date part
            TotalReviews = totalReviews,
            PositiveReviews = positiveReviews,
            NeutralReviews = neutralReviews,
            NegativeReviews = negativeReviews,
            RespondedReviews = respondedReviews,
            AverageRating = averageRating,
            NewReviews = newReviews
        };
    }
}
