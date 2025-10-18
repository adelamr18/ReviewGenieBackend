using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Application.Dto;

namespace ReviewGenie.WebApi.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<ReviewListDto>> GetReviews([FromQuery] ReviewFiltersDto filters)
    {
        var result = await _reviewService.GetReviewsAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
    {
        var review = await _reviewService.GetReviewAsync(id);
        if (review == null)
            return NotFound();

        return Ok(review);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
    {
        var review = await _reviewService.CreateReviewAsync(dto);
        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReviewDto>> UpdateReview(Guid id, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var review = await _reviewService.UpdateReviewAsync(id, dto);
            return Ok(review);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        try
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("generate-response")]
    public async Task<ActionResult<ReviewDto>> GenerateResponse([FromBody] GenerateResponseDto dto)
    {
        try
        {
            var review = await _reviewService.GenerateResponseAsync(dto);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<ReviewDto>> ApproveResponse(Guid id, [FromBody] string? customResponse = null)
    {
        try
        {
            var review = await _reviewService.ApproveResponseAsync(id, customResponse);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("analytics/{businessId}")]
    public async Task<ActionResult<ReviewAnalyticsDto>> GetAnalytics(Guid businessId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var analytics = await _reviewService.GetAnalyticsAsync(businessId, fromDate, toDate);
        return Ok(analytics);
    }

    [HttpPost("sync/{businessId}")]
    public async Task<ActionResult<List<ReviewDto>>> SyncReviews(Guid businessId)
    {
        var reviews = await _reviewService.SyncReviewsAsync(businessId);
        return Ok(reviews);
    }

    [HttpPost("metrics/{businessId}/calculate")]
    public async Task<ActionResult<ReviewMetricsDto>> CalculateDailyMetrics(Guid businessId, [FromBody] DateTime date)
    {
        var metrics = await _reviewService.CalculateDailyMetricsAsync(businessId, date);
        return Ok(metrics);
    }
}

