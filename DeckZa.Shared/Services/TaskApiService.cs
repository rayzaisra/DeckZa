using System.Net.Http.Json;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;
using DeckZa.Shared.Services.Interfaces;

namespace DeckZa.Shared.Services;

public class TaskApiService
{
    private readonly ApiService _api;
    private readonly ITokenStorage _tokenStorage;

    public TaskApiService(ApiService api, ITokenStorage tokenStorage)
    {
        _api = api;
        _tokenStorage = tokenStorage;
    }

    public async Task<PaginatedResponse<TaskListItemDto>> GetListAsync(TaskListRequestDto request)
        => await _api.PostRawFromJsonAsync<TaskListRequestDto, PaginatedResponse<TaskListItemDto>>("api/task/list", request)
           ?? new PaginatedResponse<TaskListItemDto>();

    public async Task<List<TaskListItemDto>> GetBoardAsync(TaskBoardRequestDto request)
        => await _api.PostFromJsonAsync<TaskBoardRequestDto, List<TaskListItemDto>>("api/task/board", request)
           ?? new List<TaskListItemDto>();

    public async Task<List<TaskListItemDto>> GetCalendarAsync(TaskCalendarRequestDto request)
        => await _api.PostFromJsonAsync<TaskCalendarRequestDto, List<TaskListItemDto>>("api/task/calendar", request)
           ?? new List<TaskListItemDto>();

    public Task<TaskDetailDto?> GetAsync(Guid taskId)
        => _api.GetFromJsonAsync<TaskDetailDto>($"api/task/{taskId}", unwrap: true);

    public async Task<List<TaskHistoryItemDto>> GetHistoryAsync(Guid taskId)
        => await _api.GetFromJsonAsync<List<TaskHistoryItemDto>>($"api/task/{taskId}/history", unwrap: true)
           ?? new List<TaskHistoryItemDto>();

    public async Task<BaseGeneralResponse> CreateAsync(TaskEditDto dto)
        => await ReadAsync(await _api.PostAsync("api/task", dto));

    public async Task<BaseGeneralResponse> UpdateAsync(Guid taskId, TaskEditDto dto)
        => await ReadAsync(await _api.PutAsync($"api/task/{taskId}", dto));

    public async Task<BaseGeneralResponse> MoveAsync(Guid taskId, string statusId)
        => await ReadAsync(await _api.PostAsync($"api/task/{taskId}/move", new TaskStatusMoveDto { StatusId = statusId }));

    public async Task<BaseGeneralResponse> DeleteAsync(Guid taskId)
        => await ReadAsync(await _api.DeleteAsync($"api/task/{taskId}"));

    public async Task<List<DropdownItemDto>> GetApplicationsAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/task-applications", unwrap: true)
           ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetTaskTypesAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/task-types", unwrap: true)
           ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetTaskPrioritiesAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/task-priorities", unwrap: true)
           ?? new List<DropdownItemDto>();

    public async Task<List<DropdownItemDto>> GetTaskStatusesAsync()
        => await _api.GetFromJsonAsync<List<DropdownItemDto>>("api/dropdown/task-statuses", unwrap: true)
           ?? new List<DropdownItemDto>();

    public async Task<string?> GetImportTemplateUrlAsync()
    {
        var baseAddress = _api.ApiBaseAddress;
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return null;
        }

        var token = await _tokenStorage.GetAccessTokenAsync();
        var query = $"?access_token={Uri.EscapeDataString(token ?? string.Empty)}";
        return $"{baseAddress}/api/task/import-template{query}";
    }

    public async Task<ApiResponse<TaskImportResultDto>?> ImportAsync(Stream stream, string fileName)
    {
        var response = await _api.PostFileAsync("api/task/import", stream, fileName);
        return await response.Content.ReadFromJsonAsync<ApiResponse<TaskImportResultDto>>();
    }

    private static async Task<BaseGeneralResponse> ReadAsync(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentLength is null or 0)
        {
            return new BaseGeneralResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Request failed.",
            };
        }

        var result = await response.Content.ReadFromJsonAsync<BaseGeneralResponse>();
        return result ?? new BaseGeneralResponse
        {
            Success = response.IsSuccessStatusCode,
            Message = response.ReasonPhrase ?? "Request failed.",
        };
    }
}
