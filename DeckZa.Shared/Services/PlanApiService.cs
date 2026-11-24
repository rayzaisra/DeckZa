using DeckZa.Application.DTOs.Plans;

namespace DeckZa.Shared.Services;

public class PlanApiService
{
    private readonly ApiService _api;

    public PlanApiService(ApiService api)
    {
        _api = api;
    }

    public Task<EffectivePlanDto?> GetMyPlanAsync()
        => _api.GetFromJsonAsync<EffectivePlanDto>("api/plan/me", unwrap: true);
}
