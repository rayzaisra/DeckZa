using DeckZa.Application.DTOs.Email;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces.Queries;

public interface IEmailProviderQueryService
{
    Task<(List<EmailProviderDto> Data, int TotalCount)> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);
}
