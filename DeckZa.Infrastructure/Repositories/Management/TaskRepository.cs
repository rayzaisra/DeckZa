using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class TaskRepository : ITaskRepository
{
    private readonly DeckZaDbContext _context;

    public TaskRepository(DeckZaDbContext context) => _context = context;

    public Task<tb_task?> GetByIdForUserAsync(
        Guid id,
        string userId,
        CancellationToken cancellationToken = default)
        => _context.tb_task.FirstOrDefaultAsync(
            e => e.id == id
                 && e.createdBy == userId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task AddAsync(tb_task entity, CancellationToken cancellationToken = default)
        => _context.tb_task.AddAsync(entity, cancellationToken).AsTask();

    public Task UpdateAsync(tb_task entity)
    {
        _context.tb_task.Update(entity);
        return Task.CompletedTask;
    }
}
