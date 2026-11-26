using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class ExportJobRepository : IExportJobRepository
{
    private readonly DeckZaDbContext _context;

    public ExportJobRepository(DeckZaDbContext context) => _context = context;

    public Task<tb_export_job?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default)
        => _context.tb_export_job.FirstOrDefaultAsync(
            e => e.job_id == jobId && e.isDeleted != true,
            cancellationToken);

    public Task<tb_export_job?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
        => _context.tb_export_job.FirstOrDefaultAsync(
            e => e.file_name == fileName && e.isDeleted != true,
            cancellationToken);

    public Task<List<tb_export_job>> GetExpiredAsync(DateTime now, CancellationToken cancellationToken = default)
        => _context.tb_export_job
            .Where(e => e.isDeleted != true && e.expires_at != null && e.expires_at <= now)
            .ToListAsync(cancellationToken);

    public Task AddAsync(tb_export_job entity, CancellationToken cancellationToken = default)
        => _context.tb_export_job.AddAsync(entity, cancellationToken).AsTask();

    public Task UpdateAsync(tb_export_job entity)
    {
        _context.tb_export_job.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(tb_export_job entity)
    {
        _context.tb_export_job.Remove(entity);
        return Task.CompletedTask;
    }
}
