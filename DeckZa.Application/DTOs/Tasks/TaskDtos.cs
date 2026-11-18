using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.DTOs.Tasks;

public class TaskListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
    public string? ApplicationName { get; set; }
    public string TaskTypeId { get; set; } = string.Empty;
    public string? TaskTypeName { get; set; }
    public string TaskPriorityId { get; set; } = string.Empty;
    public string? TaskPriorityName { get; set; }
    public string StatusId { get; set; } = string.Empty;
    public string? StatusName { get; set; }
    public int? StatusOrderSeq { get; set; }
    public string RequestBy { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? ImportBatchId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool IsOverdue { get; set; }
}

public class TaskDetailDto : TaskListItemDto
{
}

public class TaskEditDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ApplicationId { get; set; } = string.Empty;
    public string RequestBy { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string StatusId { get; set; } = string.Empty;
    public string TaskTypeId { get; set; } = string.Empty;
    public string TaskPriorityId { get; set; } = string.Empty;
}

public class TaskStatusMoveDto
{
    public string StatusId { get; set; } = string.Empty;
}

public class TaskListRequestDto : DataTableRequestDto
{
    public string? ApplicationId { get; set; }
    public string? TaskTypeId { get; set; }
    public string? TaskPriorityId { get; set; }
    public string? StatusId { get; set; }
}

public class TaskBoardRequestDto
{
    public string? ApplicationId { get; set; }
    public string? TaskTypeId { get; set; }
    public string? TaskPriorityId { get; set; }
    public string? SearchTerm { get; set; }
}

public class TaskCalendarRequestDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ApplicationId { get; set; }
    public string? TaskTypeId { get; set; }
    public string? StatusId { get; set; }
}

public class TaskHistoryItemDto
{
    public int Id { get; set; }
    public Guid TaskId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? FromStatusId { get; set; }
    public string? FromStatusName { get; set; }
    public string? ToStatusId { get; set; }
    public string? ToStatusName { get; set; }
    public string? FromPriorityId { get; set; }
    public string? FromPriorityName { get; set; }
    public string? ToPriorityId { get; set; }
    public string? ToPriorityName { get; set; }
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
}
