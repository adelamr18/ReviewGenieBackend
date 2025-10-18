namespace ReviewGenie.Domain.Entities;

public class Review
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BusinessId { get; init; }
    
    public string Platform { get; private set; } = null!; // "Google" or "Yelp"
    public string ExternalId { get; private set; } = null!; // Platform-specific review ID
    public string AuthorName { get; private set; } = null!;
    public string AuthorEmail { get; private set; } = null!;
    public int Rating { get; private set; }
    public string Text { get; private set; } = null!;
    public DateTime PostedAt { get; private set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    // AI Analysis
    public string? Sentiment { get; private set; } // "positive", "negative", "neutral"
    public string? GeneratedResponse { get; private set; }
    public bool HasResponded { get; private set; } = false;
    public DateTime? RespondedAt { get; private set; }
    public string? ResponseText { get; private set; }
    
    // Metadata
    public string? PlatformUrl { get; private set; }
    public string? AuthorPhotoUrl { get; private set; }
    public bool IsVerified { get; private set; } = false;

    public static Review Create(Guid businessId, string platform, string externalId, 
                              string authorName, string authorEmail, int rating, 
                              string text, DateTime postedAt, string? platformUrl = null, 
                              string? authorPhotoUrl = null, bool isVerified = false)
    {
        return new Review
        {
            BusinessId = businessId,
            Platform = platform,
            ExternalId = externalId,
            AuthorName = authorName,
            AuthorEmail = authorEmail,
            Rating = rating,
            Text = text,
            PostedAt = postedAt,
            PlatformUrl = platformUrl,
            AuthorPhotoUrl = authorPhotoUrl,
            IsVerified = isVerified
        };
    }

    public void SetSentiment(string sentiment)
    {
        Sentiment = sentiment;
    }

    public void SetGeneratedResponse(string response)
    {
        GeneratedResponse = response;
    }

    public void MarkAsResponded(string responseText)
    {
        HasResponded = true;
        RespondedAt = DateTime.UtcNow;
        ResponseText = responseText;
    }
}
