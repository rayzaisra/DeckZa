using System.Security.Claims;
using System.Text.RegularExpressions;

using Hangfire;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Authorization;
using DeckZa.Api.Helpers;
using DeckZa.Api.Services;
using DeckZa.Application.Common;
using DeckZa.Application.DTOs.Dynamic;
using DeckZa.Application.DTOs.Export;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Reports;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Enums;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private static readonly Regex SafeFileName = new(@"^[A-Za-z0-9_.-]+$", RegexOptions.Compiled);

    private readonly IBackgroundJobClient _jobs;
    private readonly IWebHostEnvironment _environment;
    private readonly IPermissionService _permissions;
    private readonly IExportJobTrackingService _trackingService;

    public ExportController(
        IBackgroundJobClient jobs,
        IWebHostEnvironment environment,
        IPermissionService permissions,
        IExportJobTrackingService trackingService)
    {
        _jobs = jobs;
        _environment = environment;
        _permissions = permissions;
        _trackingService = trackingService;
    }

    [HttpPost("users")]
    [RequireMenuAccess(PermissionAction.Export, "MNT-USER-LST")]
    public async Task<IActionResult> QueueUsers([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "users", "users");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportUsersAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("groups")]
    [RequireMenuAccess(PermissionAction.Export, "MNT-GRP-LST")]
    public async Task<IActionResult> QueueGroups([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "groups", "groups");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportGroupsAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("roles")]
    [RequireMenuAccess(PermissionAction.Export, "MNT-ROLE-LST")]
    public async Task<IActionResult> QueueRoles([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "roles", "roles");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportRolesAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("menus")]
    [RequireMenuAccess(PermissionAction.Export, "MNT-MENU-LST")]
    public async Task<IActionResult> QueueMenus([FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "menus", "menus");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportMenusAsync(request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("parameters/{tableName}")]
    [RequireMenuAccess(PermissionAction.Export, "MNT-PARAM-MKR")]
    public async Task<IActionResult> QueueParameters(string tableName, [FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "parameters", "parameters");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportParametersAsync(tableName, request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("master-data/{tableName}")]
    [RequireMenuAccess(PermissionAction.Export, "TSKP-APP", "TSKP-TYP", "TSKP-PRI", "TSKP-STA")]
    public async Task<IActionResult> QueueMasterData(string tableName, [FromBody] DataTableRequestDto request)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "master-data", "master_data");
        _jobs.Enqueue<ManagementExportJob>(job =>
            job.ExportMasterDataAsync(tableName, request.SearchTerm, userId, jobId, fileName));
        return Queued(jobId, fileName);
    }

    [HttpPost("report-performance")]
    [RequireMenuAccess(PermissionAction.Export, "TSK-RPT")]
    public async Task<IActionResult> QueueReportPerformance([FromBody] ReportFilterDto filter)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "report-performance", "report_performance");
        _jobs.Enqueue<ReportExportJob>(job =>
            job.ExportAsync(filter, userId, jobId, fileName));

        return Queued(jobId, fileName);
    }

    [HttpGet("status/{jobId}")]
    public async Task<IActionResult> GetExportStatus(string jobId)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (!await CanAccessJobAsync(jobId, userId))
        {
            return NotFound(ApiResponseFactory.NotFound<ExportStatusDto>("Export file not found."));
        }

        var job = await _trackingService.GetByJobIdAsync(jobId);
        var file = ResolveExportFile(job?.file_name);
        return Ok(ApiResponseFactory.Success(new ExportStatusDto
        {
            Ready = file != null,
            FileName = file == null ? null : Path.GetFileName(file),
            Message = file == null ? "Export is still processing." : "Export is ready.",
        }, file == null ? "Export is still processing." : "Export is ready."));
    }

    [HttpGet("download-export/{jobId}")]
    public async Task<IActionResult> DownloadExport(string jobId)
    {
        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (!await CanAccessJobAsync(jobId, userId))
        {
            return NotFound(ApiResponseFactory.NotFound<string>("Export file not found."));
        }

        var job = await _trackingService.GetByJobIdAsync(jobId);
        var file = ResolveExportFile(job?.file_name);
        if (file == null)
        {
            return NotFound(ApiResponseFactory.NotFound<string>("Export file not found."));
        }

        var bytes = System.IO.File.ReadAllBytes(file);
        var fileName = Path.GetFileName(file);
        System.IO.File.Delete(file);
        return File(
            bytes,
            ContentTypeFor(fileName),
            fileName);
    }

    [HttpPost("report/{reportId}")]
    public async Task<IActionResult> QueueReport(string reportId, [FromBody] DynamicLoadRequestDto request)
    {
        if (!await _permissions.HasAccessAsync(PermissionAction.Export))
        {
            return Forbid();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "dynamic-report", $"report_{Sanitize(reportId)}");
        _jobs.Enqueue<ExcelExportJob>(job =>
            job.ExportReportAsync(reportId, request, userId, jobId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("screen/{screenId}")]
    public async Task<IActionResult> QueueScreen(string screenId, [FromBody] DynamicLoadRequestDto request)
    {
        if (!await _permissions.HasScreenAccessAsync(screenId, PermissionAction.Export))
        {
            return Forbid();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "dynamic-screen", $"screen_{Sanitize(screenId)}");
        _jobs.Enqueue<ExcelExportJob>(job =>
            job.ExportScreenAsync(screenId, request, userId, jobId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("pdf/report/{reportId}")]
    public async Task<IActionResult> QueuePdfReport(string reportId, [FromBody] DynamicLoadRequestDto request)
    {
        if (!await _permissions.HasAccessAsync(PermissionAction.Export))
        {
            return Forbid();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "dynamic-report-pdf", $"report_{Sanitize(reportId)}", ".pdf");
        _jobs.Enqueue<PdfExportJob>(job =>
            job.ExportReportAsync(reportId, request, userId, jobId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpPost("pdf/screen/{screenId}")]
    public async Task<IActionResult> QueuePdfScreen(string screenId, [FromBody] DynamicLoadRequestDto request)
    {
        if (!await _permissions.HasScreenAccessAsync(screenId, PermissionAction.Export))
        {
            return Forbid();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var (jobId, fileName) = await CreateTrackedJobAsync(userId, "dynamic-screen-pdf", $"screen_{Sanitize(screenId)}", ".pdf");
        _jobs.Enqueue<PdfExportJob>(job =>
            job.ExportScreenAsync(screenId, request, userId, jobId, fileName));

        return Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> Download(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!SafeFileName.IsMatch(fileName) || extension is not (".xlsx" or ".pdf"))
        {
            return BadRequest();
        }

        var userId = CurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        if (!await _trackingService.CanAccessByFileNameAsync(fileName, userId, IsAdministrator()))
        {
            return NotFound(ApiResponseFactory.NotFound<string>("Export file not found."));
        }

        var path = Path.Combine(_environment.ContentRootPath, "exports", fileName);
        if (!System.IO.File.Exists(path))
        {
            return NotFound(ApiResponseFactory.NotFound<string>("Export file not found."));
        }

        var bytes = System.IO.File.ReadAllBytes(path);
        System.IO.File.Delete(path);
        return File(bytes, ContentTypeFor(fileName), fileName);
    }

    private string? CurrentUserId()
        => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private bool IsAdministrator()
        => User.IsInRole(Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE);

    private async Task<bool> CanAccessJobAsync(string jobId, string userId)
    {
        if (string.IsNullOrWhiteSpace(jobId) || jobId.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return false;
        }

        return await _trackingService.CanAccessAsync(jobId, userId, IsAdministrator());
    }

    private async Task<(string JobId, string FileName)> CreateTrackedJobAsync(
        string userId,
        string exportType,
        string prefix,
        string extension = ".xlsx")
    {
        var jobId = Guid.NewGuid().ToString("N");
        var fileName = $"{prefix}_{jobId}{extension}";
        await _trackingService.CreateAsync(jobId, userId, exportType, fileName);
        return (jobId, fileName);
    }

    private IActionResult Queued(string jobId, string fileName)
        => Ok(ApiResponseFactory.Success(
            new ExportQueueResultDto { JobId = jobId, FileName = fileName },
            "Export started."));

    private string? ResolveExportFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !SafeFileName.IsMatch(fileName))
        {
            return null;
        }

        var folder = Path.Combine(_environment.ContentRootPath, "exports");
        var path = Path.Combine(folder, fileName);
        return System.IO.File.Exists(path) ? path : null;
    }

    private static string ContentTypeFor(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    private static string Sanitize(string value)
    {
        var sanitized = new string(value.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "export" : sanitized;
    }
}
