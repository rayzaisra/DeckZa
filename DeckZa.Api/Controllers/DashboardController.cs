using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.Dashboard;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardWidgetService _widgetService;

    public DashboardController(IDashboardWidgetService widgetService)
        => _widgetService = widgetService;

    [HttpGet("widgets")]
    public async Task<IActionResult> GetWidgets(CancellationToken cancellationToken)
    {
        var widgets = await _widgetService.GetForCurrentUserAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(widgets, "Dashboard widgets retrieved successfully"));
    }
}
