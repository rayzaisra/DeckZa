using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Screen;

namespace DeckZa.Application.Interfaces;

public interface IScreenMenuEditorService
{
    Task<GeneralListResponseDto<ScreenMenuListItemDto>> GetListAsync(DataTableRequestDto request);
    Task<ScreenMenuEditDto?> GetAsync(string id);
    Task<BaseGeneralResponse> StageAsync(ScreenMenuEditDto dto);
}
