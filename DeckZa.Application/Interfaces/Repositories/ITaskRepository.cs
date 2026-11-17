using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface ITaskRepository
{
    Task<tb_task?> GetByIdForUserAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task AddAsync(tb_task entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(tb_task entity);
}
