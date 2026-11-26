using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Authorization;
using DeckZa.Api.Helpers;
using DeckZa.Application.Common;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DropDownController : ControllerBase
{
    private readonly IDropDownService _dropDownService;

    public DropDownController(IDropDownService dropDownService) => _dropDownService = dropDownService;

    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetGroupsAsync(), "Groups retrieved successfully"));

    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetGendersAsync(), "Genders retrieved successfully"));

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetUsersAsync(), "Users retrieved successfully"));

    [HttpGet("menu-icons")]
    public async Task<IActionResult> GetMenuIcons()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetMenuIconsAsync(), "Menu icons retrieved successfully"));

    [HttpGet("menu-parent-levels")]
    public async Task<IActionResult> GetMenuParentLevels()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetMenuParentLevelsAsync(), "Menu parent levels retrieved successfully"));

    [HttpGet("task-applications")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskApplications([FromServices] ICurrentUserService currentUser)
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskApplicationsAsync(currentUser.UserId ?? string.Empty), "Applications retrieved successfully"));

    [HttpGet("task-types")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskTypes([FromServices] ICurrentUserService currentUser)
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskTypesAsync(currentUser.UserId ?? string.Empty), "Task types retrieved successfully"));

    [HttpGet("task-priorities")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskPriorities([FromServices] ICurrentUserService currentUser)
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskPrioritiesAsync(currentUser.UserId ?? string.Empty), "Task priorities retrieved successfully"));

    [HttpGet("task-statuses")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskStatuses([FromServices] ICurrentUserService currentUser)
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskStatusesAsync(currentUser.UserId ?? string.Empty), "Task statuses retrieved successfully"));
}
