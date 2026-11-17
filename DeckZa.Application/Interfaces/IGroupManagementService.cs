using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Group;

namespace DeckZa.Application.Interfaces;

public interface IGroupManagementService
{
    Task<GeneralListResponseDto<GroupListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<GroupEditDto?> GetByIdAsync(string groupId);
    Task<BaseGeneralResponse> CreateAsync(GroupEditDto dto);
    Task<BaseGeneralResponse> UpdateAsync(string groupId, GroupEditDto dto);
    Task<BaseGeneralResponse> DeleteAsync(string groupId);
}
