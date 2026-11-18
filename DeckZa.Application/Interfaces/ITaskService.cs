using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;

namespace DeckZa.Application.Interfaces;

public interface ITaskService
{
    Task<GeneralListResponseDto<TaskListItemDto>> GetListAsync(TaskListRequestDto request, CancellationToken cancellationToken = default);
    Task<List<TaskListItemDto>> GetBoardAsync(TaskBoardRequestDto request, CancellationToken cancellationToken = default);
    Task<List<TaskListItemDto>> GetCalendarAsync(TaskCalendarRequestDto request, CancellationToken cancellationToken = default);
    Task<TaskDetailDto?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<List<TaskHistoryItemDto>> GetHistoryAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> CreateAsync(TaskEditDto dto, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> UpdateAsync(Guid taskId, TaskEditDto dto, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> MoveAsync(Guid taskId, TaskStatusMoveDto dto, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default);
}
