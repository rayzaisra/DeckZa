namespace DeckZa.Application.Interfaces.Queries;

public interface ITaskReferenceQueryService
{
    Task<bool> ApplicationExistsAsync(string applicationId, CancellationToken cancellationToken = default);
    Task<bool> StatusExistsAsync(string statusId, CancellationToken cancellationToken = default);
    Task<bool> TaskTypeExistsAsync(string taskTypeId, CancellationToken cancellationToken = default);
    Task<bool> TaskPriorityExistsAsync(string taskPriorityId, CancellationToken cancellationToken = default);
    Task<string?> GetStatusDescriptionAsync(string statusId, CancellationToken cancellationToken = default);
    Task<string?> GetPriorityDescriptionAsync(string taskPriorityId, CancellationToken cancellationToken = default);
}
