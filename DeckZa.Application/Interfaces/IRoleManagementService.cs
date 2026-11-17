using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Role;

namespace DeckZa.Application.Interfaces;

public interface IRoleManagementService
{
    Task<GeneralListResponseDto<RoleListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<RoleEditDto?> GetByIdAsync(string userId);
    Task<BaseGeneralResponse> CreateAsync(RoleEditDto dto);
    Task<BaseGeneralResponse> UpdateAsync(string userId, RoleEditDto dto);
    Task<BaseGeneralResponse> DeleteAsync(string userId);
}
