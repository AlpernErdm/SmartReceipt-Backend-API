using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentService paymentService,
        IApplicationDbContext context,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentResult>> CreatePayment([FromBody] CreatePaymentRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Geçersiz kullanıcı" });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Tutar 0'dan büyük olmalıdır" });
            }

            if (!Enum.IsDefined(typeof(PaymentProvider), request.Provider))
            {
                return BadRequest(new { message = "Geçersiz ödeme sağlayıcı" });
            }

            var paymentRequest = new CreatePaymentRequest
            {
                UserId = userId.Value,
                SubscriptionId = request.SubscriptionId,
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                Currency = request.Currency,
                Provider = request.Provider,
                PaymentMethod = request.PaymentMethod,
                Description = request.Description,
                Metadata = request.Metadata,
                CardToken = request.CardToken,
                CardUserKey = request.CardUserKey,
                CallbackUrl = request.CallbackUrl
            };

            _logger.LogInformation("Creating payment for user {UserId}, amount: {Amount}, provider: {Provider}", 
                userId.Value, request.Amount, request.Provider);

            var result = await _paymentService.CreatePaymentAsync(paymentRequest);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Payment failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new { 
                    message = result.ErrorMessage ?? "Ödeme işlemi başarısız oldu",
                    isSuccess = result.IsSuccess,
                    status = result.Status
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{paymentId}")]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentResult>> GetPaymentStatus(string paymentId)
    {
        var result = await _paymentService.GetPaymentStatusAsync(paymentId);
        return Ok(result);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<PaymentHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaymentHistoryDto>>> GetPaymentHistory()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var payments = await _context.Payments
            .Where(p => p.UserId == userId.Value)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentHistoryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                Provider = p.Provider,
                CreatedAt = p.CreatedAt,
                PaidAt = p.PaidAt
            })
            .ToListAsync();

        return Ok(payments);
    }

    [HttpPost("{paymentId}/refund")]
    [ProducesResponseType(typeof(RefundResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<RefundResult>> CreateRefund(string paymentId, [FromBody] CreateRefundRequestDto request)
    {
        var payment = await _context.Payments.FindAsync(Guid.Parse(paymentId));
        if (payment == null)
        {
            return NotFound();
        }

        var refundRequest = new CreateRefundRequest
        {
            PaymentId = payment.Id,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _paymentService.CreateRefundAsync(refundRequest);
        return Ok(result);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class CreatePaymentRequestDto
{
    public Guid? SubscriptionId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public PaymentProvider Provider { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    
    public string? CardToken { get; set; }
    public string? CardUserKey { get; set; }
    public string? CallbackUrl { get; set; }
}

public class PaymentHistoryDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentProvider Provider { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class CreateRefundRequestDto
{
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

