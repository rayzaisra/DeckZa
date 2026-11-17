using DeckZa.Application.DTOs.Reports;

namespace DeckZa.Application.Interfaces;

public interface IReportService
{
    Task<ReportResultDto?> GetAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);
}
