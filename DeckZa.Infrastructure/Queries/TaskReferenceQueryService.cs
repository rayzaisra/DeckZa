using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.Tasks;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class TaskReferenceQueryService : ITaskReferenceQueryService
{
    private readonly DeckZaDbContext _context;

    public TaskReferenceQueryService(DeckZaDbContext context) => _context = context;

    public Task<bool> ApplicationExistsAsync(string applicationId, string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_application.AnyAsync(
            e => e.application_id == applicationId
                 && e.owner_user_id == ownerUserId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> StatusExistsAsync(string statusId, string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_status.AnyAsync(
            e => e.status_id == statusId
                 && e.owner_user_id == ownerUserId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> TaskTypeExistsAsync(string taskTypeId, string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_task_type.AnyAsync(
            e => e.task_type_id == taskTypeId
                 && e.owner_user_id == ownerUserId
                 && e.isActive == true
                 && e.isDeleted == false,
            cancellationToken);

    public Task<bool> TaskPriorityExistsAsync(string taskPriorityId, string ownerUserId, CancellationToken cancellationToken = default)
        => _context.rf_task_priority.AnyAsync(
            e => e.task_priority_id == taskPriorityId
                 && e.owner_user_id == ownerUserId
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

    public async Task<TaskStatusFlagsDto> GetStatusFlagsAsync(string ownerUserId, CancellationToken cancellationToken = default)
    {
        var statuses = await _context.rf_status
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .Select(e => new TaskStatusFlagsDto
            {
                StatusId = e.status_id,
                Description = e.description,
                IsInitialStatus = e.is_initial_status,
                IsCompletedStatus = e.is_completed_status,
            })
            .ToListAsync(cancellationToken);

        return new TaskStatusFlagsDto
        {
            StatusId = statuses.FirstOrDefault(e => e.IsInitialStatus)?.StatusId ?? string.Empty,
            Description = statuses.FirstOrDefault(e => e.IsInitialStatus)?.Description ?? string.Empty,
            IsInitialStatus = statuses.Any(e => e.IsInitialStatus),
            IsCompletedStatus = statuses.Any(e => e.IsCompletedStatus),
        };
    }

    public async Task<Dictionary<string, TaskStatusFlagsDto>> GetStatusFlagsMapAsync(string ownerUserId, CancellationToken cancellationToken = default)
    {
        var statuses = await _context.rf_status
            .AsNoTracking()
            .Where(e => e.owner_user_id == ownerUserId && e.isActive == true && e.isDeleted == false)
            .Select(e => new TaskStatusFlagsDto
            {
                StatusId = e.status_id,
                Description = e.description,
                IsInitialStatus = e.is_initial_status,
                IsCompletedStatus = e.is_completed_status,
            })
            .ToListAsync(cancellationToken);

        return statuses.ToDictionary(e => e.StatusId, StringComparer.OrdinalIgnoreCase);
    }
}
