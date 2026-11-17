using DeckZa.Application.DTOs.Reports;

namespace DeckZa.Shared.Services;

public class ReportApiService
{
    private readonly ApiService _api;

    public ReportApiService(ApiService api) => _api = api;

    public Task<ReportResultDto?> PreviewAsync(ReportFilterDto filter)
        => _api.PostFromJsonAsync<ReportFilterDto, ReportResultDto>("api/report/preview", filter);
}
