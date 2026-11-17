using DeckZa.Application.DTOs.Dynamic;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Application.Interfaces;

public interface IDynamicEngineService
{
    Task<DynamicScreenDto?> GetScreenAsync(string screenId);
    Task<bool> CanAccessScreenAsync(string screenId);
    Task<DynamicLoadResultDto> LoadScreenAsync(string screenId, DynamicLoadRequestDto request);
    Task<DynamicLoadResultDto> LoadScreenAsUserAsync(string screenId, DynamicLoadRequestDto request, string userId);
    Task<List<DynamicOptionDto>> GetOptionsAsync(string screenId, string fieldId);
    Task<Dictionary<string, object?>?> GetChildDetailAsync(string screenId, string key);
    Task<BaseGeneralResponse> SaveChildAsync(string screenId, DynamicMutationRequestDto request);
    Task<BaseGeneralResponse> ProcessAsync(string screenId, DynamicMutationRequestDto request);
    Task<BaseGeneralResponse> DeleteAsync(string screenId, DynamicMutationRequestDto request);
    Task<BaseGeneralResponse> ImportExcelAsync(string screenId, Stream stream, string fileName);
    Task<DynamicExportResultDto> ExportTxtAsync(string screenId, DynamicLoadRequestDto request);

    Task<DynamicReportDto?> GetReportAsync(string reportId);
    Task<DynamicLoadResultDto> LoadReportAsync(string reportId, DynamicLoadRequestDto request);
    Task<DynamicLoadResultDto> LoadReportAsUserAsync(string reportId, DynamicLoadRequestDto request, string userId);
}
