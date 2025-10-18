using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;

namespace ReviewGenie.Infrastructure.Services;

public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not configured");
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> AnalyzeSentimentAsync(string reviewText)
    {
        try
        {
            var prompt = $@"Analyze the sentiment of this review and respond with only one word: positive, negative, or neutral.

Review: ""{reviewText}""

Sentiment:";

            var response = await CallOpenAIAsync(prompt, 0.1);
            var sentiment = response.Trim().ToLower();
            
            // Validate response
            if (sentiment is "positive" or "negative" or "neutral")
                return sentiment;
            
            // Fallback to neutral if response is unclear
            return "neutral";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment for review: {ReviewText}", reviewText);
            return "neutral";
        }
    }

    public async Task<string> GenerateResponseAsync(GenerateResponseDto request)
    {
        try
        {
            var review = await GetReviewForGenerationAsync(request.ReviewId);
            if (review == null)
                throw new ArgumentException("Review not found");

            var prompt = BuildResponsePrompt(review, request.CustomPrompt, request.BusinessKeywords);
            var response = await CallOpenAIAsync(prompt, 0.6);
            
            return response.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response for review {ReviewId}", request.ReviewId);
            throw;
        }
    }

    public async Task<(string sentiment, string response)> AnalyzeAndGenerateResponseAsync(GenerateResponseDto request)
    {
        try
        {
            var review = await GetReviewForGenerationAsync(request.ReviewId);
            if (review == null)
                throw new ArgumentException("Review not found");

            var sentiment = await AnalyzeSentimentAsync(review.Text);
            var prompt = BuildResponsePrompt(review, request.CustomPrompt, request.BusinessKeywords);
            var response = await CallOpenAIAsync(prompt, 0.6);
            
            return (sentiment, response.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing and generating response for review {ReviewId}", request.ReviewId);
            throw;
        }
    }

    private async Task<string> CallOpenAIAsync(string prompt, double temperature)
    {
        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that analyzes reviews and generates professional, on-brand responses for local businesses." },
                new { role = "user", content = prompt }
            },
            temperature = temperature,
            max_tokens = 200
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_baseUrl, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
    }

    private string BuildResponsePrompt(ReviewDto review, string? customPrompt, string? businessKeywords)
    {
        var basePrompt = $@"Generate a professional, warm, and on-brand response to this {review.Platform} review for a local business.

Review Details:
- Rating: {review.Rating}/5 stars
- Author: {review.AuthorName}
- Text: ""{review.Text}""
- Sentiment: {review.Sentiment ?? "unknown"}";

        if (!string.IsNullOrEmpty(businessKeywords))
        {
            basePrompt += $"\n- Business Keywords to include: {businessKeywords}";
        }

        if (!string.IsNullOrEmpty(customPrompt))
        {
            basePrompt += $"\n- Custom Instructions: {customPrompt}";
        }

        basePrompt += @"

Guidelines:
- Keep response under 80 words
- Be warm, professional, and authentic
- Address specific points mentioned in the review
- For positive reviews: thank them and invite them back
- For negative reviews: apologize, acknowledge concerns, and offer to make it right
- For neutral reviews: thank them and encourage them to return
- Include relevant business keywords naturally if provided
- End with a call to action when appropriate

Response:";

        return basePrompt;
    }

    private Task<ReviewDto?> GetReviewForGenerationAsync(Guid reviewId)
    {
        // This would typically come from a service, but for now we'll return null
        // In a real implementation, you'd inject IReviewService and call it
        // For now, we'll create a mock review for testing
        return Task.FromResult<ReviewDto?>(new ReviewDto(
            reviewId,
            Guid.NewGuid(),
            "Google",
            "test_review",
            "Test Author",
            "test@email.com",
            5,
            "This is a test review",
            DateTime.UtcNow,
            DateTime.UtcNow,
            "positive",
            null,
            false,
            null,
            null,
            null,
            null,
            true
        ));
    }
}
