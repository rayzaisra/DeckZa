using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class TaskImportRepository : ITaskImportRepository
{
    private readonly DeckZaDbContext _context;

    public TaskImportRepository(DeckZaDbContext context) => _context = context;

    public Task<tb_import_batch?> GetBatchByHashAsync(
        string fileHash,
        string importBy,
        CancellationToken cancellationToken = default)
        => _context.tb_import_batch.FirstOrDefaultAsync(
            e => e.file_hash == fileHash
                 && e.import_by == importBy
                 && e.isDeleted != true,
            cancellationToken);

    public Task<List<rf_application>> GetActiveApplicationsAsync(string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_application
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .ToListAsync(cancellationToken);

    public Task<List<rf_status>> GetActiveStatusesAsync(string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_status
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .ToListAsync(cancellationToken);

    public Task<List<rf_task_type>> GetActiveTaskTypesAsync(string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_task_type
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .ToListAsync(cancellationToken);

    public Task<List<rf_task_priority>> GetActiveTaskPrioritiesAsync(string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_task_priority
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .ToListAsync(cancellationToken);

    public Task AddBatchAsync(tb_import_batch entity, CancellationToken cancellationToken = default)
        => _context.tb_import_batch.AddAsync(entity, cancellationToken).AsTask();

    public Task AddTaskAsync(tb_task entity, CancellationToken cancellationToken = default)
        => _context.tb_task.AddAsync(entity, cancellationToken).AsTask();
}
