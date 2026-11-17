using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Screen;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/screen-menu-editor")]
[Authorize(Roles = "00")]
public class ScreenMenuEditorController : ControllerBase
{
    private readonly IScreenMenuEditorService _screenMenuService;

    public ScreenMenuEditorController(IScreenMenuEditorService screenMenuService)
        => _screenMenuService = screenMenuService;

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] DataTableRequestDto request)
    {
        var result = await _screenMenuService.GetListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Screens retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var screen = await _screenMenuService.GetAsync(id);
        return screen == null
            ? NotFound(ApiResponseFactory.NotFound<ScreenMenuEditDto>("Screen not found."))
            : Ok(ApiResponseFactory.Success(screen, "Screen retrieved successfully"));
    }

    [HttpPost]
    public async Task<IActionResult> Stage([FromBody] ScreenMenuEditDto dto)
    {
        var result = await _screenMenuService.StageAsync(dto);
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
