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
    public string? ProofContentType { get; set; }
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

/// <summary>
/// Baris daftar pengajuan pembayaran untuk halaman maintenance.
/// </summary>
public sealed class PaymentApprovalListItemDto
{
    public long Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string? PlanName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IDR";
    public string? TransferReference { get; set; }
    public string ProofFileName { get; set; } = string.Empty;
    public string ProofContentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? ActivatedFrom { get; set; }
    public DateTime? ActivatedTo { get; set; }

    public long? InvoiceId { get; set; }
    public string? InvoiceNo { get; set; }

    /// <summary>1 = Approve, 2 = Pending, 0 = Reject (mengikuti UserApprovalListItemDto).</summary>
    public int SelectedAction { get; set; } = 2;
}

/// <summary>
/// Berkas bukti pembayaran milik satu pengajuan.
/// </summary>
public sealed class PaymentProofDto
{
    public long RequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long SizeBytes { get; set; }

    /// <summary>Path absolut di disk, untuk dibaca oleh controller.</summary>
    public string AbsolutePath { get; set; } = string.Empty;
}
