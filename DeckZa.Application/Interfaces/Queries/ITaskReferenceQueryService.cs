using DeckZa.Application.DTOs.Tasks;

namespace DeckZa.Application.Interfaces.Queries;

public interface ITaskReferenceQueryService
{
    Task<bool> ApplicationExistsAsync(string applicationId, string ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> StatusExistsAsync(string statusId, string ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> TaskTypeExistsAsync(string taskTypeId, string ownerUserId, CancellationToken cancellationToken = default);
    Task<bool> TaskPriorityExistsAsync(string taskPriorityId, string ownerUserId, CancellationToken cancellationToken = default);
    Task<string?> GetStatusDescriptionAsync(string statusId, CancellationToken cancellationToken = default);
    Task<string?> GetPriorityDescriptionAsync(string taskPriorityId, CancellationToken cancellationToken = default);
    Task<TaskStatusFlagsDto> GetStatusFlagsAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, TaskStatusFlagsDto>> GetStatusFlagsMapAsync(string ownerUserId, CancellationToken cancellationToken = default);
}
