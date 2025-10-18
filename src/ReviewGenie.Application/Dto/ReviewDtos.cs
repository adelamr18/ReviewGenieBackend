namespace ReviewGenie.Application.Dto;

public record ReviewDto(
    Guid Id,
    Guid BusinessId,
    string Platform,
    string ExternalId,
    string AuthorName,
    string AuthorEmail,
    int Rating,
    string Text,
    DateTime PostedAt,
    DateTime CreatedAt,
    string? Sentiment,
    string? GeneratedResponse,
    bool HasResponded,
    DateTime? RespondedAt,
    string? ResponseText,
    string? PlatformUrl,
    string? AuthorPhotoUrl,
    bool IsVerified
);

public record CreateReviewDto(
    Guid BusinessId,
    string Platform,
    string ExternalId,
    string AuthorName,
    string AuthorEmail,
    int Rating,
    string Text,
    DateTime PostedAt,
    string? PlatformUrl = null,
    string? AuthorPhotoUrl = null,
    bool IsVerified = false
);

public record UpdateReviewDto(
    string? Sentiment = null,
    string? GeneratedResponse = null,
    bool? HasResponded = null,
    string? ResponseText = null
);

public record ReviewListDto(
    List<ReviewDto> Reviews,
    int TotalCount,
    int PageNumber,
    int PageSize
);

public record ReviewFiltersDto(
    Guid? BusinessId = null,
    string? Platform = null,
    string? Sentiment = null,
    bool? HasResponded = null,
    int? MinRating = null,
    int? MaxRating = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20
);

public record GenerateResponseDto(
    Guid ReviewId,
    string? CustomPrompt = null,
    string? BusinessKeywords = null
);

public record ReviewAnalyticsDto(
    int TotalReviews,
    int RespondedReviews,
    double ResponseRate,
    double AverageRating,
    int PositiveReviews,
    int NeutralReviews,
    int NegativeReviews,
    List<ReviewMetricsDto> DailyMetrics
);

public record ReviewMetricsDto(
    DateTime Date,
    int TotalReviews,
    int PositiveReviews,
    int NeutralReviews,
    int NegativeReviews,
    int RespondedReviews,
    double AverageRating,
    int NewReviews
);
