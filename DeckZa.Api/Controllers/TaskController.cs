using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "01")]
public class TaskController : ControllerBase
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly ITaskService _taskService;
    private readonly ITaskImportService _taskImportService;

    public TaskController(ITaskService taskService, ITaskImportService taskImportService)
    {
        _taskService = taskService;
        _taskImportService = taskImportService;
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetList([FromBody] TaskListRequestDto request)
    {
        var result = await _taskService.GetListAsync(request);
        return Ok(ApiResponseFactory.Page(result.Data, request, result.TotalCount, "Tasks retrieved successfully"));
    }

    [HttpPost("board")]
    public async Task<IActionResult> GetBoard([FromBody] TaskBoardRequestDto request)
        => Ok(ApiResponseFactory.Success(await _taskService.GetBoardAsync(request), "Task board retrieved successfully"));

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> Get(Guid taskId)
    {
        var task = await _taskService.GetByIdAsync(taskId);
        return task == null
            ? NotFound(ApiResponseFactory.NotFound<TaskDetailDto>("Task not found."))
            : Ok(ApiResponseFactory.Success(task, "Task retrieved successfully"));
    }

    [HttpGet("{taskId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid taskId)
        => Ok(ApiResponseFactory.Success(await _taskService.GetHistoryAsync(taskId), "Task history retrieved successfully"));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskEditDto dto)
        => ToActionResult(await _taskService.CreateAsync(dto));

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid taskId, [FromBody] TaskEditDto dto)
        => ToActionResult(await _taskService.UpdateAsync(taskId, dto));

    [HttpPost("{taskId:guid}/move")]
    public async Task<IActionResult> Move(Guid taskId, [FromBody] TaskStatusMoveDto dto)
        => ToActionResult(await _taskService.MoveAsync(taskId, dto));

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid taskId)
        => ToActionResult(await _taskService.DeleteAsync(taskId));

    [HttpGet("import-template")]
    public async Task<IActionResult> GetImportTemplate(CancellationToken cancellationToken)
    {
        var template = await _taskImportService.BuildTemplateAsync(cancellationToken);
        return File(template.Content, ExcelContentType, template.FileName);
    }

    [HttpPost("import")]
    [RequestSizeLimit(1048576)]
    public async Task<IActionResult> Import([FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null)
        {
            return BadRequest(ApiResponseFactory.Error<TaskImportResultDto>("Please choose an Excel file."));
        }

        var outcome = await _taskImportService.ImportAsync(
            file.OpenReadStream(),
            file.FileName,
            file.Length,
            cancellationToken);

        return outcome.Success
            ? Ok(ApiResponseFactory.Success(outcome.Result, outcome.Message))
            : BadRequest(ApiResponseFactory.Error<TaskImportResultDto>(outcome.Message));
    }

    private IActionResult ToActionResult(BaseGeneralResponse result)
    {
        var response = ApiResponseFactory.FromResult(result);
        return result.Success ? Ok(response) : BadRequest(response);
    }
}
