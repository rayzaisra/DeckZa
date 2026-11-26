using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface IExportJobRepository
{
    Task<tb_export_job?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default);
    Task<tb_export_job?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
    Task<List<tb_export_job>> GetExpiredAsync(DateTime now, CancellationToken cancellationToken = default);
    Task AddAsync(tb_export_job entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(tb_export_job entity);
    Task RemoveAsync(tb_export_job entity);
}
