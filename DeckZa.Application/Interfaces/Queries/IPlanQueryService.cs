using DeckZa.Application.DTOs.Plans;

namespace DeckZa.Application.Interfaces.Queries;

public interface IPlanQueryService
{
    Task<List<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
}
