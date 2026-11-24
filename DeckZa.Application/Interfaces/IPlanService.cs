using DeckZa.Application.DTOs.Plans;

namespace DeckZa.Application.Interfaces;

public interface IPlanService
{
    Task<EffectivePlanDto?> GetEffectivePlanAsync(string userId, CancellationToken cancellationToken = default);
    Task<EffectivePlanDto?> GetEffectivePlanWithUsageAsync(string userId, CancellationToken cancellationToken = default);
}
