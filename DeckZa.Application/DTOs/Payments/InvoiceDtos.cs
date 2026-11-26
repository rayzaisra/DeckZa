namespace DeckZa.Application.DTOs.Payments;

/// <summary>
/// Data yang dibutuhkan untuk merender invoice PDF.
/// Dipisah dari entity agar renderer (QuestPDF) tidak bergantung pada EF Core.
/// </summary>
public sealed class InvoiceRenderModel
{
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IDR";
    public string BillingPeriod { get; set; } = "ONE_TIME";
    public DateTime? ActivatedFrom { get; set; }
    public DateTime? ActivatedTo { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "TRANSFER";
    public string? TransferReference { get; set; }
}

/// <summary>
/// Hasil penerbitan invoice. Dikembalikan agar approve flow bisa melampirkan
/// file PDF pada email tanpa perlu querying ulang.
/// </summary>
public sealed class InvoiceIssueResultDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public long InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    /// <summary>Path relatif terhadap ContentRoot, mis. <c>invoices/asep/INV-202611-0001.pdf</c>.</summary>
    public string StoredPath { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/pdf";
    public long SizeBytes { get; set; }

    /// <summary>True bila invoice sudah ada sebelumnya dan tidak dirender ulang.</summary>
    public bool AlreadyIssued { get; set; }
}

/// <summary>Baris invoice milik user untuk halaman Billing.</summary>
public sealed class PaymentInvoiceDto
{
    public long Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IDR";
    public string Status { get; set; } = string.Empty;
}

/// <summary>Berkas invoice PDF yang sudah divalidasi aksesnya.</summary>
public sealed class PaymentInvoiceFileDto
{
    public long InvoiceId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public string AbsolutePath { get; set; } = string.Empty;
}

/// <summary>Hasil akses unduhan invoice: sukses, tidak ditemukan, atau terlarang.</summary>
public sealed class PaymentInvoiceFileResultDto
{
    public bool Success { get; set; }
    public bool Forbidden { get; set; }
    public string? Message { get; set; }
    public PaymentInvoiceFileDto? File { get; set; }

    public static PaymentInvoiceFileResultDto Ok(PaymentInvoiceFileDto file)
        => new() { Success = true, File = file };

    public static PaymentInvoiceFileResultDto NotFound(string message = "Invoice not found.")
        => new() { Message = message };

    public static PaymentInvoiceFileResultDto Denied(string message = "Access denied.")
        => new() { Forbidden = true, Message = message };
}
