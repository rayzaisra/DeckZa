using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface ITaskImportRepository
{
    Task<tb_import_batch?> GetBatchByHashAsync(string fileHash, string userId, CancellationToken cancellationToken = default);
    Task<List<rf_application>> GetActiveApplicationsAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<List<rf_status>> GetActiveStatusesAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<List<rf_task_type>> GetActiveTaskTypesAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task<List<rf_task_priority>> GetActiveTaskPrioritiesAsync(string ownerUserId, CancellationToken cancellationToken = default);
    Task AddBatchAsync(tb_import_batch batch, CancellationToken cancellationToken = default);
    Task AddTaskAsync(tb_task task, CancellationToken cancellationToken = default);
}
