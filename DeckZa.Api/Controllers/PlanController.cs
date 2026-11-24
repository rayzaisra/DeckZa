using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.Plans;
using DeckZa.Application.Interfaces;
using DeckZa.Application.Interfaces.Queries;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanController : ControllerBase
{
    private readonly IPlanQueryService _planQueryService;
    private readonly IPlanService _planService;
    private readonly ICurrentUserService _currentUser;

    public PlanController(
        IPlanQueryService planQueryService,
        IPlanService planService,
        ICurrentUserService currentUser)
    {
        _planQueryService = planQueryService;
        _planService = planService;
        _currentUser = currentUser;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetPlans(CancellationToken cancellationToken)
    {
        var plans = await _planQueryService.GetActivePlansAsync(cancellationToken);
        return Ok(ApiResponseFactory.Success(plans, "Plans retrieved successfully"));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyPlan(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponseFactory.Error<EffectivePlanDto>("User session is not valid."));
        }

        var plan = await _planService.GetEffectivePlanWithUsageAsync(userId, cancellationToken);
        return plan == null
            ? NotFound(ApiResponseFactory.NotFound<EffectivePlanDto>("Plan not found."))
            : Ok(ApiResponseFactory.Success(plan, "Plan retrieved successfully"));
    }
}
