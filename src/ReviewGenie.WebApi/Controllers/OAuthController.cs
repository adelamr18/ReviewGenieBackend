using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewGenie.Application.Contracts;
using System.Security.Claims;

namespace ReviewGenie.WebApi.Controllers;

[ApiController]
[Route("api/oauth")]
[Authorize]
public class OAuthController : ControllerBase
{
    private readonly IGoogleBusinessService _googleService;
    private readonly IBusinessIntegrationRepository _integrationRepo;
    private readonly IBusinessRepository _businessRepo;

    public OAuthController(
        IGoogleBusinessService googleService,
        IBusinessIntegrationRepository integrationRepo,
        IBusinessRepository businessRepo)
    {
        _googleService = googleService;
        _integrationRepo = integrationRepo;
        _businessRepo = businessRepo;
    }

    [HttpGet("google/start")]
    public async Task<IActionResult> StartGoogleOAuth([FromQuery] Guid businessId)
    {
        var userId = GetCurrentUserId();
        var business = await _businessRepo.GetAsync(businessId);
        
        if (business == null || business.OwnerId != userId)
        {
            return Unauthorized(new { error = "Business not found or access denied" });
        }

        var redirectUri = $"{Request.Scheme}://{Request.Host}/api/oauth/google/callback";
        var authUrl = await _googleService.GetAuthorizationUrlAsync(businessId, redirectUri);

        return Ok(new { authUrl });
    }

    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleOAuthCallback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return BadRequest("Missing code or state parameter");
        }

        if (!Guid.TryParse(state, out var businessId))
        {
            return BadRequest("Invalid state parameter");
        }

        try
        {
            var redirectUri = $"{Request.Scheme}://{Request.Host}/api/oauth/google/callback";
            var integration = await _googleService.ExchangeCodeForTokensAsync(code, businessId, redirectUri);
            
            var existingIntegration = await _integrationRepo.GetByBusinessAndPlatformAsync(businessId, "google");
            if (existingIntegration != null)
            {
                existingIntegration.AccessToken = integration.AccessToken;
                existingIntegration.RefreshToken = integration.RefreshToken;
                existingIntegration.ExpiresAt = integration.ExpiresAt;
                existingIntegration.ExternalAccountId = integration.ExternalAccountId;
                existingIntegration.ExternalLocationId = integration.ExternalLocationId;
                existingIntegration.BusinessName = integration.BusinessName;
                existingIntegration.BusinessAddress = integration.BusinessAddress;
                existingIntegration.IsActive = true;
                
                await _integrationRepo.UpdateAsync(existingIntegration);
            }
            else
            {
                await _integrationRepo.CreateAsync(integration);
            }

            var frontendUrl = "http://localhost:8080/settings?connected=google";
            return Redirect(frontendUrl);
        }
        catch (Exception ex)
        {
            var errorUrl = $"http://localhost:8080/settings?error={Uri.EscapeDataString(ex.Message)}";
            return Redirect(errorUrl);
        }
    }

    [HttpPost("google/disconnect")]
    public async Task<IActionResult> DisconnectGoogle([FromBody] DisconnectRequest request)
    {
        var userId = GetCurrentUserId();
        var business = await _businessRepo.GetAsync(request.BusinessId);
        
        if (business == null || business.OwnerId != userId)
        {
            return Unauthorized(new { error = "Business not found or access denied" });
        }

        var integration = await _integrationRepo.GetByBusinessAndPlatformAsync(request.BusinessId, "google");
        if (integration != null)
        {
            await _integrationRepo.DeleteAsync(integration.Id);
        }

        return Ok(new { message = "Google Business Profile disconnected successfully" });
    }

    [HttpGet("integrations/{businessId}")]
    public async Task<IActionResult> GetIntegrations(Guid businessId)
    {
        var userId = GetCurrentUserId();
        var business = await _businessRepo.GetAsync(businessId);
        
        if (business == null || business.OwnerId != userId)
        {
            return Unauthorized(new { error = "Business not found or access denied" });
        }

        var integrations = await _integrationRepo.GetByBusinessIdAsync(businessId);
        
        var result = integrations.Select(i => new
        {
            i.Id,
            i.Platform,
            i.BusinessName,
            i.BusinessAddress,
            i.ConnectedAt,
            i.LastSyncAt,
            IsExpired = DateTime.UtcNow >= i.ExpiresAt
        });

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(userIdClaim!);
    }
}

public class DisconnectRequest
{
    public Guid BusinessId { get; set; }
}
