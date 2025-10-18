using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;
using System.Text.Json;

namespace ReviewGenie.Infrastructure.Services;

public class GoogleMyBusinessService : IGoogleMyBusinessService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleMyBusinessService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public GoogleMyBusinessService(HttpClient httpClient, IConfiguration configuration, ILogger<GoogleMyBusinessService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _clientId = _configuration["ExternalPlatforms:GoogleMyBusiness:ClientId"] ?? throw new InvalidOperationException("Google My Business Client ID not configured");
        _clientSecret = _configuration["ExternalPlatforms:GoogleMyBusiness:ClientSecret"] ?? throw new InvalidOperationException("Google My Business Client Secret not configured");
    }

    public async Task<string> GetAccessTokenAsync(string refreshToken)
    {
        try
        {
            var requestBody = new
            {
                client_id = _clientId,
                client_secret = _clientSecret,
                refresh_token = refreshToken,
                grant_type = "refresh_token"
            };

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("grant_type", "refresh_token")
            });

            var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return tokenResponse.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("Failed to get access token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google My Business access token");
            throw;
        }
    }

    public async Task<List<CreateReviewDto>> GetReviewsAsync(string accessToken, string locationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var response = await _httpClient.GetAsync($"https://mybusiness.googleapis.com/v4/accounts/{locationId}/locations/{locationId}/reviews");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var reviewsResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            var reviews = new List<CreateReviewDto>();
            
            if (reviewsResponse.TryGetProperty("reviews", out var reviewsArray))
            {
                foreach (var reviewElement in reviewsArray.EnumerateArray())
                {
                    var review = new CreateReviewDto(
                        Guid.NewGuid(), // BusinessId - you'll need to map this
                        "Google",
                        reviewElement.GetProperty("reviewId").GetString() ?? "",
                        reviewElement.GetProperty("reviewer").GetProperty("displayName").GetString() ?? "",
                        reviewElement.GetProperty("reviewer").GetProperty("profilePhotoUrl").GetString() ?? "",
                        reviewElement.GetProperty("starRating").GetString() == "FIVE" ? 5 :
                        reviewElement.GetProperty("starRating").GetString() == "FOUR" ? 4 :
                        reviewElement.GetProperty("starRating").GetString() == "THREE" ? 3 :
                        reviewElement.GetProperty("starRating").GetString() == "TWO" ? 2 : 1,
                        reviewElement.GetProperty("comment").GetString() ?? "",
                        DateTime.Parse(reviewElement.GetProperty("createTime").GetString() ?? DateTime.UtcNow.ToString()),
                        reviewElement.GetProperty("reviewUrl").GetString(),
                        reviewElement.GetProperty("reviewer").GetProperty("profilePhotoUrl").GetString(),
                        true
                    );
                    reviews.Add(review);
                }
            }

            return reviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Google My Business reviews");
            throw;
        }
    }

    public async Task<bool> PostResponseAsync(string accessToken, string reviewName, string response)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            
            var requestBody = new
            {
                comment = response
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync($"https://mybusiness.googleapis.com/v4/{reviewName}/reply", content);
            
            return httpResponse.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting response to Google My Business review");
            return false;
        }
    }

    public async Task<List<CreateReviewDto>> SyncReviewsAsync(Guid businessId)
    {
        // This would require storing the location ID and refresh token for each business
        // For now, return empty list - you'll need to implement business-specific logic
        _logger.LogInformation("Google My Business sync not fully implemented - requires business-specific location ID and refresh token");
        return new List<CreateReviewDto>();
    }

    public async Task<bool> PostResponseAsync(Guid businessId, string reviewExternalId, string response)
    {
        // This would require storing the access token for each business
        _logger.LogInformation("Google My Business response posting not fully implemented - requires business-specific access token");
        return false;
    }

    public async Task<bool> ValidateCredentialsAsync(Guid businessId)
    {
        // This would validate the stored credentials for the business
        _logger.LogInformation("Google My Business credential validation not fully implemented");
        return false;
    }
}

