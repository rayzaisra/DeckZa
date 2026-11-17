using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Menu;

namespace DeckZa.Application.Interfaces;

public interface IMenuManagementService
{
    Task<GeneralListResponseDto<MenuListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<MenuEditDto?> GetByIdAsync(long id);
    Task<BaseGeneralResponse> CreateAsync(MenuEditDto dto);
    Task<BaseGeneralResponse> UpdateAsync(long id, MenuEditDto dto);
    Task<BaseGeneralResponse> DeleteAsync(long id);
    Task<List<MenuTreeItemDto>> GetTreeAsync();
    Task<List<long>> GetAssignedMenuIdsAsync(string groupId);
    Task<BaseGeneralResponse> SaveMenuMapAsync(MenuMapDto dto);
}
