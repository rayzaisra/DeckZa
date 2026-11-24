using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.Plans;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class PlanQueryService : IPlanQueryService
{
    private readonly DeckZaDbContext _context;

    public PlanQueryService(DeckZaDbContext context) => _context = context;

    public async Task<List<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _context.rf_plan
            .AsNoTracking()
            .Where(e => e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.plan_name)
            .Select(e => new PlanDto
            {
                Id = e.id,
                PlanName = e.plan_name,
                Description = e.description,
                PriceAmount = e.price_amount,
                PriceCurrency = e.price_currency,
                BillingPeriod = e.billing_period,
                BillingIntervalCount = e.billing_interval_count,
                MaxActiveTasks = e.max_active_tasks,
                ReportPeriodMonths = e.report_period_months,
                AllowsCalendar = e.allows_calendar,
                AllowsExcelImport = e.allows_excel_import,
                AllowsSupervisorMode = e.allows_supervisor_mode,
                UpgradeGroupId = e.upgrade_group_id,
            })
            .ToListAsync(cancellationToken);

        if (plans.Count == 0)
        {
            return plans;
        }

        var planIds = plans.Select(e => e.Id).ToList();
        var features = await _context.rf_plan_feature
            .AsNoTracking()
            .Where(e => planIds.Contains(e.plan_id)
                        && e.isActive == true
                        && e.isDeleted == false)
            .OrderBy(e => e.plan_id)
            .ThenBy(e => e.order_seq)
            .Select(e => new { e.plan_id, e.feature_text })
            .ToListAsync(cancellationToken);

        var featureLookup = features
            .GroupBy(e => e.plan_id)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.feature_text).ToList());

        foreach (var plan in plans)
        {
            plan.Features = featureLookup.TryGetValue(plan.Id, out var items)
                ? items
                : new List<string>();
        }

        return plans;
    }
}
