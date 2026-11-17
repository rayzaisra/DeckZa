using DeckZa.Application.DTOs.Reports;

namespace DeckZa.Application.Interfaces.Queries;

public interface IReportQueryService
{
    Task<ReportResultDto> BuildAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);
}
