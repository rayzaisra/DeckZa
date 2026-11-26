using DeckZa.Application.DTOs.Payments;

namespace DeckZa.Application.Interfaces;

/// <summary>
/// Penerbitan dan akses invoice pembayaran.
/// Issue dipanggil setelah approve di luar transaksi; kegagalan render tidak
/// membatalkan aktivasi plan.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Menerbitkan invoice untuk sebuah pengajuan pembayaran yang sudah berstatus APPROVED.
    /// </summary>
    /// <param name="paymentRequestId">Id baris <c>tb_payment_request</c>.</param>
    /// <param name="actor">User id admin yang memproses.</param>
    /// <param name="reissue">
    /// Bila true dan invoice sudah pernah terbit, PDF dirender ulang dan baris invoice diperbarui.
    /// Bila false dan invoice sudah ada, baris lama dikembalikan tanpa render ulang
    /// (idempoten — approve kedua tidak menghasilkan invoice kedua).
    /// </param>
    Task<InvoiceIssueResultDto> IssueAsync(
        long paymentRequestId,
        string actor,
        bool reissue = false,
        CancellationToken cancellationToken = default);

    /// <summary>Daftar invoice milik user untuk halaman Billing.</summary>
    Task<List<PaymentInvoiceDto>> GetMyInvoicesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengambil berkas PDF invoice. Hanya pemilik atau Administrator yang boleh.
    /// </summary>
    Task<PaymentInvoiceFileResultDto> GetPdfFileAsync(
        long invoiceId,
        string requesterUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);
}
