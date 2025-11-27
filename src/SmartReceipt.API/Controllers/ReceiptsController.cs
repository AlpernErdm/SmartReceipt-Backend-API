using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Application.Features.Receipts.Commands.CreateReceipt;
using SmartReceipt.Application.Features.Receipts.Queries.GetReceiptById;
using SmartReceipt.Application.Features.Receipts.Queries.GetReceipts;

namespace SmartReceipt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(IMediator mediator, ILogger<ReceiptsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReceiptDto>>> GetReceipts(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? storeName,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var query = new GetReceiptsQuery
        {
            UserId = userId.Value,
            FromDate = fromDate,
            ToDate = toDate,
            StoreName = storeName,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptDto>> GetReceiptById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var result = await _mediator.Send(new GetReceiptByIdQuery(id, userId.Value));
        
        if (result == null)
        {
            return NotFound(new { message = "Fiş bulunamadı", id });
        }

        return Ok(result);
    }

    [HttpPost("scan")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptDto>> ScanReceipt(IFormFile imageFile)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        _logger.LogInformation("Fiş tarama isteği alındı. Kullanıcı: {UserId}, Dosya: {FileName}, Boyut: {Size} bytes", 
            userId.Value, imageFile.FileName, imageFile.Length);

        var command = new CreateReceiptCommand
        {
            UserId = userId.Value,
            ImageFile = imageFile,
            UseAiProcessing = true
        };

        var result = await _mediator.Send(command);
        
        return CreatedAtAction(
            nameof(GetReceiptById), 
            new { id = result.Id }, 
            result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptDto>> CreateReceipt([FromBody] CreateReceiptDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Geçersiz kullanıcı" });
        }

        var command = new CreateReceiptCommand
        {
            UserId = userId.Value,
            ManualData = request,
            UseAiProcessing = false
        };

        var result = await _mediator.Send(command);
        
        return CreatedAtAction(
            nameof(GetReceiptById), 
            new { id = result.Id }, 
            result);
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
