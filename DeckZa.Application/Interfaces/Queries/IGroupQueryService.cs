using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Group;

namespace DeckZa.Application.Interfaces.Queries;

public interface IGroupQueryService
{
    Task<GeneralListResponseDto<GroupListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);
}
