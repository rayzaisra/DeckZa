using DeckZa.Application.DTOs.Dashboard;
using DeckZa.Application.DTOs.General;

namespace DeckZa.Shared.Services;

public sealed class DashboardApiService
{
    private readonly ApiService _api;

    public DashboardApiService(ApiService api)
        => _api = api;

    public async Task<List<DashboardWidgetDto>> GetWidgetsAsync()
        => await _api.GetFromJsonAsync<List<DashboardWidgetDto>>(
               "api/dashboard/widgets",
               unwrap: true)
           ?? new List<DashboardWidgetDto>();
}
