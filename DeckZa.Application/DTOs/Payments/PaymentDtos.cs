namespace DeckZa.Application.DTOs.Payments;

public sealed class PaymentInstructionDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}

public sealed class PaymentRequestDto
{
    public long Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IDR";
    public string? TransferReference { get; set; }
    public string ProofFileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public string? ReviewNote { get; set; }
}

public sealed class PaymentSubmitResponseDto
{
    public long Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
