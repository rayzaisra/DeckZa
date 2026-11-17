using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface ITaskHistoryRepository
{
    Task<int> GetNextOrderSeqAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task AddAsync(tb_task_history entity, CancellationToken cancellationToken = default);
}
