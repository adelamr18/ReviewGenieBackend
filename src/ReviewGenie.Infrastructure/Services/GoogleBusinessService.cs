using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;

namespace ReviewGenie.Infrastructure.Services;

public class GoogleBusinessService : IGoogleBusinessService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public GoogleBusinessService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
        _clientId = _config["ExternalPlatforms:GoogleMyBusiness:ClientId"] ?? "";
        _clientSecret = _config["ExternalPlatforms:GoogleMyBusiness:ClientSecret"] ?? "";
    }

    public Task<string> GetAuthorizationUrlAsync(Guid businessId, string redirectUri)
    {
        var scopes = "https://www.googleapis.com/auth/business.manage";
        var state = businessId.ToString();
        
        var authUrl = $"https://accounts.google.com/oauth/authorize?" +
            $"client_id={_clientId}&" +
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
            $"scope={Uri.EscapeDataString(scopes)}&" +
            $"response_type=code&" +
            $"access_type=offline&" +
            $"prompt=consent&" +
            $"state={state}";

        return Task.FromResult(authUrl);
    }

    public async Task<BusinessIntegration> ExchangeCodeForTokensAsync(string code, Guid businessId, string redirectUri)
    {
        var tokenRequest = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            code = code,
            grant_type = "authorization_code",
            redirect_uri = redirectUri
        };

        var json = JsonSerializer.Serialize(tokenRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token exchange failed: {responseJson}");
        }

        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseJson);
        
        var profile = await GetBusinessProfileWithTokenAsync(tokenResponse.access_token);

        return new BusinessIntegration
        {
            BusinessId = businessId,
            Platform = "google",
            ExternalAccountId = profile?.AccountId ?? "",
            ExternalLocationId = profile?.LocationId ?? "",
            AccessToken = tokenResponse.access_token,
            RefreshToken = tokenResponse.refresh_token ?? "",
            ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in),
            Scopes = "business.manage",
            BusinessName = profile?.Name ?? "",
            BusinessAddress = profile?.Address ?? ""
        };
    }

    public async Task<BusinessIntegration> RefreshTokenAsync(BusinessIntegration integration)
    {
        var refreshRequest = new
        {
            client_id = _clientId,
            client_secret = _clientSecret,
            refresh_token = integration.RefreshToken,
            grant_type = "refresh_token"
        };

        var json = JsonSerializer.Serialize(refreshRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", content);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Token refresh failed: {responseJson}");
        }

        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseJson);

        integration.AccessToken = tokenResponse.access_token;
        integration.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

        return integration;
    }

    public async Task<List<Review>> SyncReviewsAsync(BusinessIntegration integration)
    {
        await EnsureValidTokenAsync(integration);

        var url = $"https://mybusiness.googleapis.com/v4/accounts/{integration.ExternalAccountId}/locations/{integration.ExternalLocationId}/reviews";
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {integration.AccessToken}");

        var response = await _httpClient.GetAsync(url);
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to sync reviews: {responseJson}");
        }

        var reviewsResponse = JsonSerializer.Deserialize<GoogleReviewsResponse>(responseJson);
        
        var reviews = new List<Review>();
        
        if (reviewsResponse.reviews != null)
        {
            foreach (var gr in reviewsResponse.reviews)
            {
                var review = Review.Create(
                    integration.BusinessId,
                    "Google",
                    gr.reviewId,
                    gr.reviewer?.displayName ?? "Anonymous",
                    "",
                    gr.starRating?.value ?? 0,
                    gr.comment ?? "",
                    DateTime.TryParse(gr.createTime, out var createTime) ? createTime : DateTime.UtcNow,
                    $"https://www.google.com/maps/reviews/{gr.reviewId}",
                    null,
                    true
                );
                
                review.SetSentiment(DetermineSentiment(gr.starRating?.value ?? 0));
                
                if (!string.IsNullOrEmpty(gr.reviewReply?.comment))
                {
                    review.MarkAsResponded(gr.reviewReply.comment);
                }
                
                reviews.Add(review);
            }
        }
        
        return reviews;
    }

    public async Task<bool> PostReviewReplyAsync(BusinessIntegration integration, string reviewId, string responseText)
    {
        await EnsureValidTokenAsync(integration);

        var url = $"https://mybusiness.googleapis.com/v4/accounts/{integration.ExternalAccountId}/locations/{integration.ExternalLocationId}/reviews/{reviewId}/reply";
        
        var replyRequest = new
        {
            comment = responseText
        };

        var json = JsonSerializer.Serialize(replyRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {integration.AccessToken}");

        var response = await _httpClient.PutAsync(url, content);
        
        return response.IsSuccessStatusCode;
    }

    public async Task<GoogleBusinessProfile?> GetBusinessProfileAsync(BusinessIntegration integration)
    {
        await EnsureValidTokenAsync(integration);
        return await GetBusinessProfileWithTokenAsync(integration.AccessToken);
    }

    private async Task<GoogleBusinessProfile?> GetBusinessProfileWithTokenAsync(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var accountsResponse = await _httpClient.GetAsync("https://mybusiness.googleapis.com/v4/accounts");
        var accountsJson = await accountsResponse.Content.ReadAsStringAsync();

        if (!accountsResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var accounts = JsonSerializer.Deserialize<GoogleAccountsResponse>(accountsJson);
        var account = accounts.accounts?.FirstOrDefault();
        
        if (account == null) return null;

        var locationsResponse = await _httpClient.GetAsync($"https://mybusiness.googleapis.com/v4/{account.name}/locations");
        var locationsJson = await locationsResponse.Content.ReadAsStringAsync();

        if (!locationsResponse.IsSuccessStatusCode)
        {
            return null;
        }

        var locations = JsonSerializer.Deserialize<GoogleLocationsResponse>(locationsJson);
        var location = locations.locations?.FirstOrDefault();

        if (location == null) return null;

        return new GoogleBusinessProfile
        {
            AccountId = account.name.Split('/').Last(),
            LocationId = location.name.Split('/').Last(),
            Name = location.locationName ?? "",
            Address = location.address?.addressLines?.FirstOrDefault() ?? "",
            PhoneNumber = location.primaryPhone ?? "",
            WebsiteUrl = location.websiteUrl ?? ""
        };
    }

    private async Task EnsureValidTokenAsync(BusinessIntegration integration)
    {
        if (DateTime.UtcNow >= integration.ExpiresAt.AddMinutes(-5))
        {
            await RefreshTokenAsync(integration);
        }
    }

    private static string DetermineSentiment(int rating)
    {
        return rating switch
        {
            >= 4 => "positive",
            3 => "neutral",
            _ => "negative"
        };
    }
}

public class GoogleTokenResponse
{
    public string access_token { get; set; } = "";
    public string? refresh_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; } = "";
}

public class GoogleAccountsResponse
{
    public GoogleAccount[]? accounts { get; set; }
}

public class GoogleAccount
{
    public string name { get; set; } = "";
    public string accountName { get; set; } = "";
}

public class GoogleLocationsResponse
{
    public GoogleLocation[]? locations { get; set; }
}

public class GoogleLocation
{
    public string name { get; set; } = "";
    public string? locationName { get; set; }
    public GoogleAddress? address { get; set; }
    public string? primaryPhone { get; set; }
    public string? websiteUrl { get; set; }
}

public class GoogleAddress
{
    public string[]? addressLines { get; set; }
}

public class GoogleReviewsResponse
{
    public GoogleReview[]? reviews { get; set; }
}

public class GoogleReview
{
    public string reviewId { get; set; } = "";
    public GoogleReviewer? reviewer { get; set; }
    public GoogleStarRating? starRating { get; set; }
    public string? comment { get; set; }
    public string createTime { get; set; } = "";
    public GoogleReviewReply? reviewReply { get; set; }
}

public class GoogleReviewer
{
    public string? displayName { get; set; }
}

public class GoogleStarRating
{
    public int value { get; set; }
}

public class GoogleReviewReply
{
    public string? comment { get; set; }
    public string? updateTime { get; set; }
}
