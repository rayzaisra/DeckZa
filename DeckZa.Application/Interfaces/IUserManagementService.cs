using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.User;

namespace DeckZa.Application.Interfaces;

public interface IUserManagementService
{
    Task<GeneralListResponseDto<UserListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<GeneralListResponseDto<UserFlagDto>> GetUserFlagListAsync(DataTableRequestDto request);
    Task<UserEditDto?> GetByIdAsync(string userId);
    Task<BaseGeneralResponse> CreateAsync(UserEditDto dto);
    Task<BaseGeneralResponse> UpdateAsync(string userId, UserEditDto dto);
    Task<BaseGeneralResponse> DeleteAsync(string userId);
    Task<BaseGeneralResponse> UnlockAsync(string userId);
    Task<BaseGeneralResponse> ResetPasswordAsync(string userId, string newPassword);
}

public interface IDropDownService
{
    Task<List<DropdownItemDto>> GetGroupsAsync();
    Task<List<DropdownItemDto>> GetGendersAsync();
    Task<List<DropdownItemDto>> GetUsersAsync();
    Task<List<DropdownItemDto>> GetMenuIconsAsync();
    Task<List<DropdownItemDto>> GetMenuParentLevelsAsync();
    Task<List<DropdownItemDto>> GetTaskApplicationsAsync();
    Task<List<DropdownItemDto>> GetTaskTypesAsync();
    Task<List<DropdownItemDto>> GetTaskPrioritiesAsync();
    Task<List<DropdownItemDto>> GetTaskStatusesAsync();
}
