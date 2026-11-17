using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Screen;

namespace DeckZa.Application.Interfaces.Queries;

public interface IScreenMenuQueryService
{
    Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ScreenMenuEditDto?> GetAsync(string id, CancellationToken cancellationToken = default);
}
