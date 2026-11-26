namespace DeckZa.Application.DTOs.General;

/// <summary>
/// Lampiran file untuk email keluar. Disimpan pada tiga kolom lampiran di
/// <c>tb_email_outbox</c> (file name, content type, storage path) lalu dibaca
/// <see cref="Interfaces.IEmailDispatchService"/> saat proses pengiriman.
/// </summary>
public class EmailAttachmentDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Path file lampiran. Relatif terhadap ContentRoot (pola yang sama dengan
    /// <c>tb_payment_request.proof_stored_path</c>) atau absolut.
    /// </summary>
    public string StoredPath { get; set; } = string.Empty;
}
