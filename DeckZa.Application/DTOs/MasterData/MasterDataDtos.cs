namespace DeckZa.Application.DTOs.MasterData;

public sealed class MasterItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderSeq { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsInitialStatus { get; set; }
    public bool IsCompletedStatus { get; set; }
    public int StatusApproval { get; set; }
}
