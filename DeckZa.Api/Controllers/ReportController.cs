using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Authorization;
using DeckZa.Api.Helpers;
using DeckZa.Application.Common;
using DeckZa.Application.DTOs.Reports;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireMenuAccess(PermissionAction.Load, "TSK-RPT")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService) => _reportService = reportService;

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] ReportFilterDto filter)
    {
        var report = await _reportService.GetAsync(filter);
        return report == null
            ? Unauthorized(ApiResponseFactory.Error<ReportResultDto>("User session is not valid."))
            : Ok(ApiResponseFactory.Success(report, "Report generated successfully"));
    }
}
