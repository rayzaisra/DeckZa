using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Menu;

namespace DeckZa.Application.Interfaces.Queries;

public interface IMenuQueryService
{
    Task<GeneralListResponseDto<MenuListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<MenuTreeItemDto>> GetTreeAsync(CancellationToken cancellationToken = default);

    Task<List<long>> GetAssignedMenuIdsAsync(
        string groupId,
        CancellationToken cancellationToken = default);

    Task<List<MenuItemDto>> GetMenuTreeByGroupIdAsync(
        string groupId,
        CancellationToken cancellationToken = default);
}
