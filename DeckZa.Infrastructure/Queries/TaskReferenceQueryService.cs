using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Queries;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class TaskReferenceQueryService : ITaskReferenceQueryService
{
    private readonly DeckZaDbContext _context;

    public TaskReferenceQueryService(DeckZaDbContext context) => _context = context;

    public Task<bool> ApplicationExistsAsync(string applicationId, CancellationToken cancellationToken = default)
        => _context.rf_application.AnyAsync(
            e => e.application_id == applicationId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> StatusExistsAsync(string statusId, CancellationToken cancellationToken = default)
        => _context.rf_status.AnyAsync(
            e => e.status_id == statusId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> TaskTypeExistsAsync(string taskTypeId, CancellationToken cancellationToken = default)
        => _context.rf_task_type.AnyAsync(
            e => e.task_type_id == taskTypeId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> TaskPriorityExistsAsync(string taskPriorityId, CancellationToken cancellationToken = default)
        => _context.rf_task_priority.AnyAsync(
            e => e.task_priority_id == taskPriorityId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<string?> GetStatusDescriptionAsync(string statusId, CancellationToken cancellationToken = default)
        => _context.rf_status
            .Where(e => e.status_id == statusId)
            .Select(e => e.description)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<string?> GetPriorityDescriptionAsync(string taskPriorityId, CancellationToken cancellationToken = default)
        => _context.rf_task_priority
            .Where(e => e.task_priority_id == taskPriorityId)
            .Select(e => e.description)
            .FirstOrDefaultAsync(cancellationToken);
}
