using DeckZa.Application.DTOs.Approval;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IApprovalService
{
    Task<GeneralListResponseDto<UserApprovalListItemDto>> GetPendingUsersAsync(DataTableRequestDto request);
    Task<BaseGeneralResponse> ApproveUserAsync(long id);
    Task<BaseGeneralResponse> RejectUserAsync(long id);

    Task<GeneralListResponseDto<GroupApprovalListItemDto>> GetPendingGroupsAsync(DataTableRequestDto request);
    Task<BaseGeneralResponse> ApproveGroupAsync(long id);
    Task<BaseGeneralResponse> RejectGroupAsync(long id);

    Task<GeneralListResponseDto<RoleApprovalListItemDto>> GetPendingRolesAsync(DataTableRequestDto request);
    Task<BaseGeneralResponse> ApproveRoleAsync(Guid id);
    Task<BaseGeneralResponse> RejectRoleAsync(Guid id);

    Task<GeneralListResponseDto<MenuApprovalListItemDto>> GetPendingMenusAsync(DataTableRequestDto request);
    Task<BaseGeneralResponse> ApproveMenuAsync(Guid id);
    Task<BaseGeneralResponse> RejectMenuAsync(Guid id);
}
