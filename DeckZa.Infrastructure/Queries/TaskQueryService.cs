using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class TaskQueryService : ITaskQueryService
{
    private readonly DeckZaDbContext _context;

    public TaskQueryService(DeckZaDbContext context) => _context = context;

    public async Task<GeneralListResponseDto<TaskListItemDto>> GetListAsync(
        string userId,
        TaskListRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = ApplyFilters(BuildQuery(userId), request);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.Title.Contains(search)
                || x.RequestBy.Contains(search));
        }

        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
            "applicationname" => descending ? query.OrderByDescending(x => x.ApplicationName) : query.OrderBy(x => x.ApplicationName),
            "tasktypename" => descending ? query.OrderByDescending(x => x.TaskTypeName) : query.OrderBy(x => x.TaskTypeName),
            "taskpriorityname" => descending ? query.OrderByDescending(x => x.TaskPriorityName) : query.OrderBy(x => x.TaskPriorityName),
            "statusname" => descending ? query.OrderByDescending(x => x.StatusName) : query.OrderBy(x => x.StatusName),
            "duedate" => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate),
            _ => descending ? query.OrderByDescending(x => x.RequestDate) : query.OrderBy(x => x.RequestDate),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<TaskListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }

    public Task<List<TaskListItemDto>> GetBoardAsync(
        string userId,
        TaskBoardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(userId)
            .Where(x => request.ApplicationId == null || x.ApplicationId == request.ApplicationId)
            .Where(x => request.TaskTypeId == null || x.TaskTypeId == request.TaskTypeId)
            .Where(x => request.TaskPriorityId == null || x.TaskPriorityId == request.TaskPriorityId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.Title.Contains(search)
                || x.RequestBy.Contains(search));
        }

        return query
            .OrderBy(x => x.StatusOrderSeq)
            .ThenBy(x => x.DueDate)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<TaskDetailDto?> GetDetailAsync(
        string userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(userId).Where(x => x.Id == taskId);

        return query
            .Select(x => new TaskDetailDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                ApplicationId = x.ApplicationId,
                ApplicationName = x.ApplicationName,
                TaskTypeId = x.TaskTypeId,
                TaskTypeName = x.TaskTypeName,
                TaskPriorityId = x.TaskPriorityId,
                TaskPriorityName = x.TaskPriorityName,
                StatusId = x.StatusId,
                StatusName = x.StatusName,
                StatusOrderSeq = x.StatusOrderSeq,
                RequestBy = x.RequestBy,
                RequestDate = x.RequestDate,
                DueDate = x.DueDate,
                CompletedDate = x.CompletedDate,
                ImportBatchId = x.ImportBatchId,
                CreatedDate = x.CreatedDate,
                IsOverdue = x.IsOverdue,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<TaskHistoryItemDto>> GetHistoryAsync(
        string userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var query = from history in _context.tb_task_history
                    join task in _context.tb_task on history.task_id equals task.id
                    join fromStatus in _context.rf_status
                        on history.from_status_id equals fromStatus.status_id into fromStatusJoin
                    from fromStatus in fromStatusJoin.DefaultIfEmpty()
                    join toStatus in _context.rf_status
                        on history.to_status_id equals toStatus.status_id into toStatusJoin
                    from toStatus in toStatusJoin.DefaultIfEmpty()
                    join fromPriority in _context.rf_task_priority
                        on history.from_priority_id equals fromPriority.task_priority_id into fromPriorityJoin
                    from fromPriority in fromPriorityJoin.DefaultIfEmpty()
                    join toPriority in _context.rf_task_priority
                        on history.to_priority_id equals toPriority.task_priority_id into toPriorityJoin
                    from toPriority in toPriorityJoin.DefaultIfEmpty()
                    where history.task_id == taskId
                          && task.createdBy == userId
                          && task.isActive == true
                          && task.isDeleted == false
                    orderby history.order_seq, history.id
                    select new TaskHistoryItemDto
                    {
                        Id = history.id,
                        TaskId = history.task_id,
                        ChangeType = history.change_type,
                        FromStatusId = history.from_status_id,
                        FromStatusName = fromStatus != null ? fromStatus.description : null,
                        ToStatusId = history.to_status_id,
                        ToStatusName = toStatus != null ? toStatus.description : null,
                        FromPriorityId = history.from_priority_id,
                        FromPriorityName = fromPriority != null ? fromPriority.description : null,
                        ToPriorityId = history.to_priority_id,
                        ToPriorityName = toPriority != null ? toPriority.description : null,
                        FieldName = history.field_name,
                        OldValue = history.old_value,
                        NewValue = history.new_value,
                        Notes = history.notes,
                        CreatedBy = history.createdBy,
                        CreatedDate = history.createdDate,
                    };

        return query.ToListAsync(cancellationToken);
    }

    private IQueryable<TaskListItemDto> BuildQuery(string userId)
    {
        var now = DateTime.Now;

        return from task in _context.tb_task
               join application in _context.rf_application
                   on task.application_id equals application.application_id into applicationJoin
               from application in applicationJoin.DefaultIfEmpty()
               join taskType in _context.rf_task_type
                   on task.task_type_id equals taskType.task_type_id into taskTypeJoin
               from taskType in taskTypeJoin.DefaultIfEmpty()
               join taskPriority in _context.rf_task_priority
                   on task.task_priority_id equals taskPriority.task_priority_id into taskPriorityJoin
               from taskPriority in taskPriorityJoin.DefaultIfEmpty()
               join status in _context.rf_status
                   on task.status_id equals status.status_id into statusJoin
               from status in statusJoin.DefaultIfEmpty()
               where task.createdBy == userId
                     && task.isActive == true
                     && task.isDeleted == false
               select new TaskListItemDto
               {
                   Id = task.id,
                   Title = task.title,
                   Description = task.description,
                   ApplicationId = task.application_id,
                   ApplicationName = application != null ? application.description : null,
                   TaskTypeId = task.task_type_id,
                   TaskTypeName = taskType != null ? taskType.description : null,
                   TaskPriorityId = task.task_priority_id,
                   TaskPriorityName = taskPriority != null ? taskPriority.description : null,
                   StatusId = task.status_id,
                   StatusName = status != null ? status.description : null,
                   StatusOrderSeq = status != null ? status.order_seq : null,
                   RequestBy = task.request_by,
                   RequestDate = task.request_date,
                   DueDate = task.due_date,
                   CompletedDate = task.completed_date,
                   ImportBatchId = task.import_batch_id,
                   CreatedDate = task.createdDate,
                   IsOverdue = task.due_date != null
                               && task.due_date < now
                               && task.status_id != "STS005",
               };
    }

    private static IQueryable<TaskListItemDto> ApplyFilters(
        IQueryable<TaskListItemDto> query,
        TaskListRequestDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.ApplicationId))
        {
            query = query.Where(x => x.ApplicationId == request.ApplicationId);
        }

        if (!string.IsNullOrWhiteSpace(request.TaskTypeId))
        {
            query = query.Where(x => x.TaskTypeId == request.TaskTypeId);
        }

        if (!string.IsNullOrWhiteSpace(request.TaskPriorityId))
        {
            query = query.Where(x => x.TaskPriorityId == request.TaskPriorityId);
        }

        if (!string.IsNullOrWhiteSpace(request.StatusId))
        {
            query = query.Where(x => x.StatusId == request.StatusId);
        }

        return query;
    }
}
