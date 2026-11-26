using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Authorization;
using DeckZa.Api.Helpers;
using DeckZa.Application.Common;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.MasterData;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Enums;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/master-data")]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _service;
    private readonly ICurrentUserService _currentUser;

    public MasterDataController(IMasterDataService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("{table}/list")]
    [RequireMenuAccess(PermissionAction.Load, "TSKP-APP", "TSKP-TYP", "TSKP-PRI", "TSKP-STA")]
    public async Task<IActionResult> GetList(string table, [FromBody] DataTableRequestDto request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<object>("User session is not valid.", Const.SESSION_EXPIRED_CODE));
        }

        var result = await _service.GetListAsync(table, userId, request, cancellationToken);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Master data retrieved successfully"));
    }

    [HttpGet("{table}")]
    [RequireMenuAccess(PermissionAction.Load, "TSKP-APP", "TSKP-TYP", "TSKP-PRI", "TSKP-STA")]
    public async Task<IActionResult> Get(string table, [FromQuery] string? search, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<object>("User session is not valid.", Const.SESSION_EXPIRED_CODE));
        }

        var items = await _service.GetAllAsync(table, userId, search, cancellationToken);
        return Ok(ApiResponseFactory.Success(items, "Master data retrieved successfully"));
    }

    [HttpPost("{table}")]
    [RequireMenuAccess(PermissionAction.AddEdit, "TSKP-APP", "TSKP-TYP", "TSKP-PRI", "TSKP-STA")]
    public async Task<IActionResult> Save(string table, [FromBody] MasterItemDto item, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<object>("User session is not valid.", Const.SESSION_EXPIRED_CODE));
        }

        var response = await _service.SaveAsync(table, item, userId, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{table}/{id}")]
    [RequireMenuAccess(PermissionAction.Delete, "TSKP-APP", "TSKP-TYP", "TSKP-PRI", "TSKP-STA")]
    public async Task<IActionResult> Delete(string table, string id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<object>("User session is not valid.", Const.SESSION_EXPIRED_CODE));
        }

        var response = await _service.DeleteAsync(table, id, userId, cancellationToken);
        return Ok(response);
    }
}
