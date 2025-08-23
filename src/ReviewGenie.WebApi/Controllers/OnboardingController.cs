using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.DTO;
using System.Security.Claims;

namespace ReviewGenie.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("onboarding")]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _svc;
    public OnboardingController(IOnboardingService s) => _svc = s;

    private Guid CurrentUserId =>
        Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")!
        );

    [HttpPost("business")]
    public async Task<IActionResult> AddBusiness(BusinessDto dto)
    {
        var bizId = await _svc.AddBusinessAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetBusiness), new { businessId = bizId },
                               new { businessId = bizId });
    }

    [HttpPost("business/{businessId:guid}/platforms")]
    public async Task<IActionResult> AddPlatforms(Guid businessId, PlatformDto dto)
    {
        await _svc.AddPlatformsAsync(CurrentUserId, businessId, dto);
        return Ok();
    }

    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetBusiness(Guid businessId)
    {
        var biz = await _svc.GetBusinessAsync(CurrentUserId, businessId);
        return biz is null ? Forbid() : Ok(biz);
    }
}
