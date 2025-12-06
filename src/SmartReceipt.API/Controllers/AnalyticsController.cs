using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;

namespace SmartReceipt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(CategoryAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryAnalyticsDto>> GetCategoryAnalytics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _analyticsService.GetCategoryAnalyticsAsync(
            userId.Value, 
            fromDate, 
            toDate);

        return Ok(result);
    }

    [HttpGet("trends")]
    [ProducesResponseType(typeof(TrendAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TrendAnalyticsDto>> GetTrendAnalytics(
        [FromQuery] int? period = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var trendPeriod = period.HasValue && Enum.IsDefined(typeof(TrendPeriod), period.Value)
            ? (TrendPeriod)period.Value
            : TrendPeriod.Monthly;

        try
        {
            var result = await _analyticsService.GetTrendAnalyticsAsync(userId.Value, trendPeriod);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trend analytics for user {UserId}", userId.Value);
            return StatusCode(500, new { message = "Analitik verileri alınırken bir hata oluştu", error = ex.Message });
        }
    }

    [HttpGet("stores")]
    [ProducesResponseType(typeof(StoreAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreAnalyticsDto>> GetStoreAnalytics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int topCount = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _analyticsService.GetStoreAnalyticsAsync(
            userId.Value, 
            fromDate, 
            toDate, 
            topCount);

        return Ok(result);
    }

    [HttpGet("tax-report")]
    [ProducesResponseType(typeof(TaxReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaxReportDto>> GetTaxReport(
        [FromQuery] int year,
        [FromQuery] int? month = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _analyticsService.GetTaxReportAsync(userId.Value, year, month);
        return Ok(result);
    }

    [HttpGet("comparison")]
    [ProducesResponseType(typeof(ComparisonAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ComparisonAnalyticsDto>> GetComparisonAnalytics(
        [FromQuery] int type)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (!Enum.IsDefined(typeof(ComparisonType), type))
        {
            return BadRequest(new { message = "Geçersiz comparison type. 1=MonthOverMonth, 2=YearOverYear" });
        }

        var comparisonType = (ComparisonType)type;

        try
        {
            var result = await _analyticsService.GetComparisonAnalyticsAsync(userId.Value, comparisonType);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comparison analytics for user {UserId}", userId.Value);
            return StatusCode(500, new { message = "Karşılaştırma analizi alınırken bir hata oluştu", error = ex.Message });
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

