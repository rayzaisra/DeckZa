using DeckZa.Application.DTOs.Email;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IEmailProviderService
{
    Task<(List<EmailProviderDto> Data, int TotalCount)> GetListAsync(DataTableRequestDto request);
    Task<EmailProviderDto?> GetAsync(int id);
    Task<BaseGeneralResponse> SaveAsync(EmailProviderDto request);
    Task<BaseGeneralResponse> DeleteAsync(int id);
    Task<BaseGeneralResponse> TestAsync(int id, string? recipient);
}
