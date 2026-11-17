using DeckZa.Application.DTOs.Reports;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;

namespace DeckZa.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IReportQueryService _reportQueryService;
    private readonly ICurrentUserService _currentUser;

    public ReportService(
        IReportQueryService reportQueryService,
        ICurrentUserService currentUser)
    {
        _reportQueryService = reportQueryService;
        _currentUser = currentUser;
    }

    public Task<ReportResultDto?> GetAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult<ReportResultDto?>(null);
        }

        return GetReportAsync(userId, filter, cancellationToken);
    }

    private async Task<ReportResultDto?> GetReportAsync(
        string userId,
        ReportFilterDto filter,
        CancellationToken cancellationToken)
        => await _reportQueryService.BuildAsync(userId, filter, cancellationToken);
}
