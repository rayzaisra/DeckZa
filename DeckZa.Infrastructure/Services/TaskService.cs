using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Tasks;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Services;

public class TaskService : ITaskService
{
    private const string DefaultStatusId = "STS001";
    private const string DoneStatusId = "STS005";
    private const int HistoryTextMaxLength = 1000;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskQueryService _queryService;
    private readonly ITaskReferenceQueryService _referenceQueryService;
    private readonly ICurrentUserService _currentUser;
    private readonly IErrorLogService _errorLog;

    public TaskService(
        IUnitOfWork unitOfWork,
        ITaskQueryService queryService,
        ITaskReferenceQueryService referenceQueryService,
        ICurrentUserService currentUser,
        IErrorLogService errorLog)
    {
        _unitOfWork = unitOfWork;
        _queryService = queryService;
        _referenceQueryService = referenceQueryService;
        _currentUser = currentUser;
        _errorLog = errorLog;
    }

    public async Task<GeneralListResponseDto<TaskListItemDto>> GetListAsync(
        TaskListRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new GeneralListResponseDto<TaskListItemDto>();
        }

        return await _queryService.GetListAsync(userId, request, cancellationToken);
    }

    public async Task<List<TaskListItemDto>> GetBoardAsync(
        TaskBoardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<TaskListItemDto>();
        }

        return await _queryService.GetBoardAsync(userId, request, cancellationToken);
    }

    public async Task<List<TaskListItemDto>> GetCalendarAsync(
        TaskCalendarRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<TaskListItemDto>();
        }

        return await _queryService.GetCalendarAsync(userId, request, cancellationToken);
    }

    public async Task<TaskDetailDto?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _queryService.GetDetailAsync(userId, taskId, cancellationToken);
    }

    public async Task<List<TaskHistoryItemDto>> GetHistoryAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<TaskHistoryItemDto>();
        }

        return await _queryService.GetHistoryAsync(userId, taskId, cancellationToken);
    }

    public async Task<BaseGeneralResponse> CreateAsync(TaskEditDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("User session is not valid.");
            }

            var title = (dto.Title ?? string.Empty).Trim();
            var requestBy = (dto.RequestBy ?? string.Empty).Trim();
            var applicationId = (dto.ApplicationId ?? string.Empty).Trim();
            var taskTypeId = (dto.TaskTypeId ?? string.Empty).Trim();
            var taskPriorityId = (dto.TaskPriorityId ?? string.Empty).Trim();
            var statusId = string.IsNullOrWhiteSpace(dto.StatusId) ? DefaultStatusId : dto.StatusId.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return Fail("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(requestBy))
            {
                return Fail("Request By is required.");
            }

            if (dto.RequestDate == default)
            {
                return Fail("Request Date is required.");
            }

            var validationError = await ValidateReferencesAsync(
                applicationId,
                statusId,
                taskTypeId,
                taskPriorityId,
                cancellationToken);

            if (validationError != null)
            {
                return Fail(validationError);
            }

            var now = DateTime.Now;
            var task = new tb_task
            {
                id = Guid.NewGuid(),
                application_id = applicationId,
                title = title,
                description = dto.Description,
                request_by = requestBy,
                request_date = dto.RequestDate,
                due_date = dto.DueDate,
                status_id = statusId,
                task_type_id = taskTypeId,
                task_priority_id = taskPriorityId,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
                approvedBy = userId,
                approvedDate = now,
                approvalTypeId = Const.ApprovalType.ADD,
            };

            if (statusId == DoneStatusId)
            {
                task.completed_date = now;
                task.due_date ??= now;
            }

            await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Success("Task created successfully.");
        }
        catch (Exception exception)
        {
            await LogErrorAsync(exception);
            return Fail("Failed to create the task.");
        }
    }

    public async Task<BaseGeneralResponse> UpdateAsync(
        Guid taskId,
        TaskEditDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("User session is not valid.");
            }

            var task = await _unitOfWork.Tasks.GetByIdForUserAsync(taskId, userId, cancellationToken);
            if (task == null)
            {
                return Fail("Task not found.");
            }

            var title = (dto.Title ?? string.Empty).Trim();
            var requestBy = (dto.RequestBy ?? string.Empty).Trim();
            var applicationId = (dto.ApplicationId ?? string.Empty).Trim();
            var taskTypeId = (dto.TaskTypeId ?? string.Empty).Trim();
            var taskPriorityId = (dto.TaskPriorityId ?? string.Empty).Trim();
            var statusId = string.IsNullOrWhiteSpace(dto.StatusId) ? task.status_id : dto.StatusId.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                return Fail("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(requestBy))
            {
                return Fail("Request By is required.");
            }

            if (dto.RequestDate == default)
            {
                return Fail("Request Date is required.");
            }

            var validationError = await ValidateReferencesAsync(
                applicationId,
                statusId,
                taskTypeId,
                taskPriorityId,
                cancellationToken);

            if (validationError != null)
            {
                return Fail(validationError);
            }

            var previousApplicationId = task.application_id;
            var previousTitle = task.title;
            var previousDescription = task.description;
            var previousRequestBy = task.request_by;
            var previousRequestDate = task.request_date;
            var previousDueDate = task.due_date;
            var previousStatusId = task.status_id;
            var previousTaskTypeId = task.task_type_id;
            var previousTaskPriorityId = task.task_priority_id;

            var now = DateTime.Now;
            var historyRows = new List<tb_task_history>();
            var orderSeq = await _unitOfWork.TaskHistory.GetNextOrderSeqAsync(task.id, cancellationToken);

            if (!string.Equals(previousStatusId, statusId, StringComparison.Ordinal))
            {
                var previousStatusName = await _referenceQueryService.GetStatusDescriptionAsync(previousStatusId, cancellationToken);
                var statusName = await _referenceQueryService.GetStatusDescriptionAsync(statusId, cancellationToken);

                if (statusId == DoneStatusId)
                {
                    task.completed_date = now;
                    task.due_date ??= now;
                }
                else if (previousStatusId == DoneStatusId)
                {
                    task.completed_date = null;
                }

                historyRows.Add(BuildHistory(
                    task.id,
                    orderSeq++,
                    "STATUS",
                    userId,
                    now,
                    fromStatusId: previousStatusId,
                    toStatusId: statusId,
                    notes: $"perpindahan dari status [{previousStatusName ?? previousStatusId}] ke status [{statusName ?? statusId}]"));
            }

            if (!string.Equals(previousTaskPriorityId, taskPriorityId, StringComparison.Ordinal))
            {
                var previousPriorityName = await _referenceQueryService.GetPriorityDescriptionAsync(previousTaskPriorityId, cancellationToken);
                var priorityName = await _referenceQueryService.GetPriorityDescriptionAsync(taskPriorityId, cancellationToken);

                historyRows.Add(BuildHistory(
                    task.id,
                    orderSeq++,
                    "PRIORITY",
                    userId,
                    now,
                    fromPriorityId: previousTaskPriorityId,
                    toPriorityId: taskPriorityId,
                    notes: $"perpindahan dari priority [{previousPriorityName ?? previousTaskPriorityId}] ke priority [{priorityName ?? taskPriorityId}]"));
            }

            AddFieldHistory(historyRows, task.id, ref orderSeq, "title", previousTitle, title, userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "description", previousDescription, dto.Description, userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "request_by", previousRequestBy, requestBy, userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "request_date", FormatDate(previousRequestDate), FormatDate(dto.RequestDate), userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "due_date", FormatDate(previousDueDate), FormatDate(dto.DueDate), userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "application_id", previousApplicationId, applicationId, userId, now);
            AddFieldHistory(historyRows, task.id, ref orderSeq, "task_type_id", previousTaskTypeId, taskTypeId, userId, now);

            task.application_id = applicationId;
            task.title = title;
            task.description = dto.Description;
            task.request_by = requestBy;
            task.request_date = dto.RequestDate;
            task.due_date = dto.DueDate;
            task.status_id = statusId;
            task.task_type_id = taskTypeId;
            task.task_priority_id = taskPriorityId;
            task.modifiedBy = userId;
            task.modifiedDate = now;

            await _unitOfWork.Tasks.UpdateAsync(task);

            foreach (var history in historyRows)
            {
                await _unitOfWork.TaskHistory.AddAsync(history, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Success("Task updated successfully.");
        }
        catch (Exception exception)
        {
            await LogErrorAsync(exception);
            return Fail("Failed to update the task.");
        }
    }

    public async Task<BaseGeneralResponse> MoveAsync(
        Guid taskId,
        TaskStatusMoveDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("User session is not valid.");
            }

            var statusId = (dto.StatusId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return Fail("Status is required.");
            }

            if (!await _referenceQueryService.StatusExistsAsync(statusId, cancellationToken))
            {
                return Fail("Status is not valid or inactive.");
            }

            var task = await _unitOfWork.Tasks.GetByIdForUserAsync(taskId, userId, cancellationToken);
            if (task == null)
            {
                return Fail("Task not found.");
            }

            if (string.Equals(task.status_id, statusId, StringComparison.Ordinal))
            {
                return Success("Task status is unchanged.");
            }

            var previousStatusId = task.status_id;
            var previousStatusName = await _referenceQueryService.GetStatusDescriptionAsync(previousStatusId, cancellationToken);
            var statusName = await _referenceQueryService.GetStatusDescriptionAsync(statusId, cancellationToken);

            var now = DateTime.Now;

            if (statusId == DoneStatusId)
            {
                task.completed_date = now;
                task.due_date ??= now;
            }
            else if (previousStatusId == DoneStatusId)
            {
                task.completed_date = null;
            }

            task.status_id = statusId;
            task.modifiedBy = userId;
            task.modifiedDate = now;

            await _unitOfWork.Tasks.UpdateAsync(task);

            var orderSeq = await _unitOfWork.TaskHistory.GetNextOrderSeqAsync(task.id, cancellationToken);
            await _unitOfWork.TaskHistory.AddAsync(
                BuildHistory(
                    task.id,
                    orderSeq,
                    "STATUS",
                    userId,
                    now,
                    fromStatusId: previousStatusId,
                    toStatusId: statusId,
                    notes: $"perpindahan dari status [{previousStatusName ?? previousStatusId}] ke status [{statusName ?? statusId}]"),
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Success("Task status updated successfully.");
        }
        catch (Exception exception)
        {
            await LogErrorAsync(exception);
            return Fail("Failed to move the task.");
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _currentUser.UserId;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Fail("User session is not valid.");
            }

            var task = await _unitOfWork.Tasks.GetByIdForUserAsync(taskId, userId, cancellationToken);
            if (task == null)
            {
                return Fail("Task not found.");
            }

            var now = DateTime.Now;
            task.isActive = false;
            task.isDeleted = true;
            task.statusApproval = Const.StatusApproval.DELETED;
            task.deletedBy = userId;
            task.deletedDate = now;
            task.modifiedBy = userId;
            task.modifiedDate = now;

            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Success("Task deleted successfully.");
        }
        catch (Exception exception)
        {
            await LogErrorAsync(exception);
            return Fail("Failed to delete the task.");
        }
    }

    private async Task<string?> ValidateReferencesAsync(
        string applicationId,
        string statusId,
        string taskTypeId,
        string taskPriorityId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(applicationId)
            || !await _referenceQueryService.ApplicationExistsAsync(applicationId, cancellationToken))
        {
            return "Application is not valid or inactive.";
        }

        if (string.IsNullOrWhiteSpace(statusId)
            || !await _referenceQueryService.StatusExistsAsync(statusId, cancellationToken))
        {
            return "Status is not valid or inactive.";
        }

        if (string.IsNullOrWhiteSpace(taskTypeId)
            || !await _referenceQueryService.TaskTypeExistsAsync(taskTypeId, cancellationToken))
        {
            return "Task Type is not valid or inactive.";
        }

        if (string.IsNullOrWhiteSpace(taskPriorityId)
            || !await _referenceQueryService.TaskPriorityExistsAsync(taskPriorityId, cancellationToken))
        {
            return "Priority is not valid or inactive.";
        }

        return null;
    }

    private static void AddFieldHistory(
        List<tb_task_history> historyRows,
        Guid taskId,
        ref int orderSeq,
        string fieldName,
        string? oldValue,
        string? newValue,
        string userId,
        DateTime now)
    {
        if (string.Equals(oldValue ?? string.Empty, newValue ?? string.Empty, StringComparison.Ordinal))
        {
            return;
        }

        historyRows.Add(BuildHistory(
            taskId,
            orderSeq++,
            "FIELD",
            userId,
            now,
            fieldName: fieldName,
            oldValue: oldValue,
            newValue: newValue,
            notes: $"{fieldName} diubah dari '{oldValue ?? string.Empty}' menjadi '{newValue ?? string.Empty}'"));
    }

    private static tb_task_history BuildHistory(
        Guid taskId,
        int orderSeq,
        string changeType,
        string userId,
        DateTime now,
        string? fromStatusId = null,
        string? toStatusId = null,
        string? fromPriorityId = null,
        string? toPriorityId = null,
        string? fieldName = null,
        string? oldValue = null,
        string? newValue = null,
        string? notes = null)
        => new()
        {
            task_id = taskId,
            from_status_id = fromStatusId,
            to_status_id = toStatusId,
            from_priority_id = fromPriorityId,
            to_priority_id = toPriorityId,
            change_type = changeType,
            field_name = fieldName,
            old_value = Truncate(oldValue),
            new_value = Truncate(newValue),
            notes = Truncate(notes),
            order_seq = orderSeq,
            statusApproval = Const.StatusApproval.APPROVED,
            isActive = true,
            isDeleted = false,
            isLocked = false,
            isDefault = false,
            createdBy = userId,
            createdDate = now,
            approvedBy = userId,
            approvedDate = now,
            approvalTypeId = Const.ApprovalType.ADD,
        };

    private static string? Truncate(string? value)
        => value != null && value.Length > HistoryTextMaxLength
            ? value[..HistoryTextMaxLength]
            : value;

    private static string? FormatDate(DateTime? value)
        => value?.ToString("yyyy-MM-dd HH:mm:ss");

    private Task LogErrorAsync(Exception exception)
        => _errorLog.LogAsync("TaskManagement", "ERROR", exception);

    private static BaseGeneralResponse Success(string message) => new()
    {
        Success = true,
        Code = Const.SUCCESS_CODE,
        Message = message,
    };

    private static BaseGeneralResponse Fail(string message) => new()
    {
        Success = false,
        Code = Const.ERROR_CODE,
        Message = message,
    };
}
