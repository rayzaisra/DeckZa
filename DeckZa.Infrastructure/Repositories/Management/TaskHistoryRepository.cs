using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class TaskHistoryRepository : ITaskHistoryRepository
{
    private readonly DeckZaDbContext _context;

    public TaskHistoryRepository(DeckZaDbContext context) => _context = context;

    public async Task<int> GetNextOrderSeqAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var current = await _context.tb_task_history
            .Where(e => e.task_id == taskId)
            .MaxAsync(e => (int?)e.order_seq, cancellationToken);

        return (current ?? 0) + 1;
    }

    public Task AddAsync(tb_task_history entity, CancellationToken cancellationToken = default)
        => _context.tb_task_history.AddAsync(entity, cancellationToken).AsTask();
}
