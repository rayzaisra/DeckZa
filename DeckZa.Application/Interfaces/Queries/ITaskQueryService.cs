using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;

namespace DeckZa.Application.Interfaces.Queries;

public interface ITaskQueryService
{
    Task<GeneralListResponseDto<TaskListItemDto>> GetListAsync(
        string userId,
        TaskListRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<TaskListItemDto>> GetBoardAsync(
        string userId,
        TaskBoardRequestDto request,
        CancellationToken cancellationToken = default);

    Task<TaskDetailDto?> GetDetailAsync(
        string userId,
        Guid taskId,
        CancellationToken cancellationToken = default);

    Task<List<TaskHistoryItemDto>> GetHistoryAsync(
        string userId,
        Guid taskId,
        CancellationToken cancellationToken = default);
}
