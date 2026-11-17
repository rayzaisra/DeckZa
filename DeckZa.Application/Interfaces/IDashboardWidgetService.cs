using DeckZa.Application.DTOs.Dashboard;

namespace DeckZa.Application.Interfaces;

public interface IDashboardWidgetService
{
    Task<List<DashboardWidgetDto>> GetForCurrentUserAsync(CancellationToken cancellationToken = default);
}
