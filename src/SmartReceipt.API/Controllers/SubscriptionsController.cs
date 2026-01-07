using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Application.Features.Subscriptions.Commands.CancelSubscription;
using SmartReceipt.Application.Features.Subscriptions.Commands.Subscribe;
using SmartReceipt.Application.Features.Subscriptions.Queries.GetCurrentSubscription;
using SmartReceipt.Application.Features.Subscriptions.Queries.GetSubscriptionPlans;
using SmartReceipt.Application.Features.Subscriptions.Queries.GetUsage;

namespace SmartReceipt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(IMediator mediator, ILogger<SubscriptionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("plans")]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans()
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery());
        return Ok(result);
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> GetCurrentSubscription()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var result = await _mediator.Send(new GetCurrentSubscriptionQuery(userId.Value));
        
        if (result == null)
        {
            return NotFound(new { message = "Aktif abonelik bulunamadı" });
        }

        return Ok(result);
    }

    [HttpGet("usage")]
    [ProducesResponseType(typeof(UsageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsageDto>> GetUsage(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var result = await _mediator.Send(new GetUsageQuery(userId.Value, year, month));
        return Ok(result);
    }

    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubscriptionDto>> Subscribe([FromBody] CreateSubscriptionDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Geçersiz kullanıcı" });
            }

            var command = new SubscribeCommand(userId.Value, request.PlanId, request.BillingPeriod);
            var result = await _mediator.Send(command);
            
            return CreatedAtAction(
                nameof(GetCurrentSubscription), 
                null, 
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subscription oluşturulurken hata: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("cancel")]
    [ProducesResponseType(typeof(SubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubscriptionDto>> CancelSubscription([FromBody] CancelSubscriptionDto? request = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var command = new CancelSubscriptionCommand(userId.Value, request?.Reason);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}

