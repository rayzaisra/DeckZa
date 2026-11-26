namespace DeckZa.Application.DTOs.Tasks;

public sealed class TaskStatusFlagsDto
{
    public string StatusId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsInitialStatus { get; set; }
    public bool IsCompletedStatus { get; set; }
}
