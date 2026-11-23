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
    public async Task<IActionResult> GetTaskApplications()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskApplicationsAsync(), "Applications retrieved successfully"));

    [HttpGet("task-types")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskTypes()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskTypesAsync(), "Task types retrieved successfully"));

    [HttpGet("task-priorities")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskPriorities()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskPrioritiesAsync(), "Task priorities retrieved successfully"));

    [HttpGet("task-statuses")]
    [RequireMenuAccess(PermissionAction.Load, "TSK-KANBAN", "TSK-CAL", "TSK-RPT")]
    public async Task<IActionResult> GetTaskStatuses()
        => Ok(ApiResponseFactory.Success(await _dropDownService.GetTaskStatusesAsync(), "Task statuses retrieved successfully"));
}
