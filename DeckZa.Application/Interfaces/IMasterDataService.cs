using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.MasterData;

namespace DeckZa.Application.Interfaces;

public interface IMasterDataService
{
    Task<GeneralListResponseDto<MasterItemDto>> GetListAsync(
        string table,
        string userId,
        DataTableRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<MasterItemDto>> GetAllAsync(
        string table,
        string userId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<BaseGeneralResponse> SaveAsync(string table, MasterItemDto item, string userId, CancellationToken cancellationToken = default);
    Task<BaseGeneralResponse> DeleteAsync(string table, string id, string userId, CancellationToken cancellationToken = default);
}
