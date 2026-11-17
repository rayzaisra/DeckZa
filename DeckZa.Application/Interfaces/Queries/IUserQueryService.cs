using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.User;

namespace DeckZa.Application.Interfaces.Queries;

public interface IUserQueryService
{
    Task<GeneralListResponseDto<UserListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GeneralListResponseDto<UserFlagDto>> GetUserFlagListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);
}
