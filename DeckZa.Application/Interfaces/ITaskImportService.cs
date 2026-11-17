using DeckZa.Application.DTOs.Tasks;

namespace DeckZa.Application.Interfaces;

public interface ITaskImportService
{
    Task<TaskImportTemplateDto> BuildTemplateAsync(CancellationToken cancellationToken = default);
    Task<TaskImportOutcomeDto> ImportAsync(
        Stream stream,
        string fileName,
        long length,
        CancellationToken cancellationToken = default);
}
