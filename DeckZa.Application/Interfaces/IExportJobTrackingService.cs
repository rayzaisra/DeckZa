using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces;

public interface IExportJobTrackingService
{
    Task<tb_export_job> CreateAsync(string jobId, string userId, string exportType, string fileName, CancellationToken cancellationToken = default);
    Task<bool> CanAccessAsync(string jobId, string userId, bool isAdministrator, CancellationToken cancellationToken = default);
    Task<bool> CanAccessByFileNameAsync(string fileName, string userId, bool isAdministrator, CancellationToken cancellationToken = default);
    Task<tb_export_job?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default);
    Task<tb_export_job?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default);
    Task MarkReadyAsync(string jobId, string fileName, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
