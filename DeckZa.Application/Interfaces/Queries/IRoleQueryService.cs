using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Role;

namespace DeckZa.Application.Interfaces.Queries;

public interface IRoleQueryService
{
    Task<GeneralListResponseDto<RoleListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);
}
