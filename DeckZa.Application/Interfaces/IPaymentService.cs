using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Payments;

namespace DeckZa.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentInstructionDto> GetInstructionsAsync(CancellationToken cancellationToken = default);
    Task<List<PaymentRequestDto>> GetMyRequestsAsync(string userId, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> SubmitAsync(
        string userId,
        string planId,
        string? transferReference,
        string fileName,
        string contentType,
        long fileSizeBytes,
        Stream fileStream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Daftar pengajuan pembayaran untuk halaman maintenance.
    /// <paramref name="status"/> kosong berarti semua status.
    /// </summary>
    Task<GeneralListResponseDto<PaymentApprovalListItemDto>> GetAdminListAsync(
        DataTableRequestDto request,
        string? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Menyetujui pengajuan: mengubah status, mengaktifkan plan, memindahkan user
    /// ke grup tujuan, lalu menerbitkan invoice dan mengirim email (di luar transaksi).
    /// </summary>
    Task<BaseGeneralResponse> ApproveAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default);

    /// <summary>Menolak pengajuan dan mengirim notifikasi + email penolakan.</summary>
    Task<BaseGeneralResponse> RejectAsync(
        long id,
        string? note,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mengambil berkas bukti pembayaran. Null bila tidak ditemukan atau pemohon
    /// bukan pemilik maupun Administrator.
    /// </summary>
    Task<PaymentProofDto?> GetProofAsync(
        long id,
        string requesterUserId,
        CancellationToken cancellationToken = default);
}
