using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using DeckZa.Api.Helpers;
using DeckZa.Application.DTOs.Plans;
using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlanController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly ICurrentUserService _currentUser;

    public PlanController(IPlanService planService, ICurrentUserService currentUser)
    {
        _planService = planService;
        _currentUser = currentUser;
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
