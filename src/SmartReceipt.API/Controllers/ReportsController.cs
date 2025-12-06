using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IReportService reportService,
        ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpPost("pdf")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GeneratePdfReport([FromBody] ReportRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reportRequest = new ReportRequest
        {
            UserId = userId.Value,
            Type = request.Type,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Year = request.Year,
            Month = request.Month,
            Category = request.Category,
            Currency = request.Currency
        };

        var pdfBytes = await _reportService.GeneratePdfReportAsync(reportRequest);
        
        return File(pdfBytes, "application/pdf", $"report_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
    }

    [HttpPost("excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateExcelReport([FromBody] ReportRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reportRequest = new ReportRequest
        {
            UserId = userId.Value,
            Type = request.Type,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Year = request.Year,
            Month = request.Month,
            Category = request.Category,
            Currency = request.Currency
        };

        var excelBytes = await _reportService.GenerateExcelReportAsync(reportRequest);
        
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpPost("csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateCsvReport([FromBody] ReportRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var reportRequest = new ReportRequest
        {
            UserId = userId.Value,
            Type = request.Type,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Year = request.Year,
            Month = request.Month,
            Category = request.Category,
            Currency = request.Currency
        };

        var csvBytes = await _reportService.GenerateCsvReportAsync(reportRequest);
        
        return File(csvBytes, "text/csv", $"report_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class ReportRequestDto
{
    public ReportType Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
}

