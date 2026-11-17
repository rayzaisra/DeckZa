using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface ITaskImportRepository
{
    Task<tb_import_batch?> GetBatchByHashAsync(string fileHash, string importBy, CancellationToken cancellationToken = default);
    Task<List<rf_application>> GetActiveApplicationsAsync(CancellationToken cancellationToken = default);
    Task<List<rf_status>> GetActiveStatusesAsync(CancellationToken cancellationToken = default);
    Task<List<rf_task_type>> GetActiveTaskTypesAsync(CancellationToken cancellationToken = default);
    Task<List<rf_task_priority>> GetActiveTaskPrioritiesAsync(CancellationToken cancellationToken = default);
    Task AddBatchAsync(tb_import_batch entity, CancellationToken cancellationToken = default);
    Task AddTaskAsync(tb_task entity, CancellationToken cancellationToken = default);
}
