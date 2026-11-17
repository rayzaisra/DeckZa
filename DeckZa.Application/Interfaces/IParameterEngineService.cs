using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Parameter;

namespace DeckZa.Application.Interfaces;

public interface IParameterEngineService
{
    Task<List<ParameterCardDto>> GetCardsAsync();
    Task<List<ParameterFieldDto>> GetFieldsAsync(string tableName);
    Task<Dictionary<string, List<DropdownItemDto>>> GetOptionsAsync(string tableName);
    Task<GeneralListResponseDto<ParameterApprovalRowDto>> GetApprovalRowsAsync(string tableName, DataTableRequestDto request);
    Task<ParameterListResponseDto> GetListAsync(string tableName, DataTableRequestDto request);
    Task<Dictionary<string, string>?> GetAsync(string tableName, string? key);
    Task<BaseGeneralResponse> SaveAsync(ParameterSaveRequestDto request);
    Task<BaseGeneralResponse> DeleteAsync(string tableName, string? key);
    Task<GeneralListResponseDto<ParameterPendingListItemDto>> GetPendingAsync(ParameterPendingRequestDto request);
    Task<BaseGeneralResponse> ApproveAsync(Guid id);
    Task<BaseGeneralResponse> RejectAsync(Guid id);
}
