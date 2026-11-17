namespace DeckZa.Application.DTOs.Tasks;

public class TaskImportOutcomeDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TaskImportResultDto? Result { get; set; }
}

public class TaskImportResultDto
{
    public Guid ImportBatchId { get; set; }
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int RejectedCount { get; set; }
    public List<TaskImportRejectionDto> Rejections { get; set; } = new();
}

public class TaskImportRejectionDto
{
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class TaskImportTemplateDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
}
