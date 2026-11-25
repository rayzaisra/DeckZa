using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.Plans;
using DeckZa.Application.Interfaces;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

public class PlanService : IPlanService
{
    private readonly DeckZaDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PlanService(DeckZaDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<EffectivePlanDto?> GetEffectivePlanAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var groupId = await _context.um_user_group
            .AsNoTracking()
            .Where(e => e.user_id == userId
                        && e.isActive == true
                        && e.isDeleted == false)
            .Select(e => e.group_id)
            .FirstOrDefaultAsync(cancellationToken);

        DeckZa.Domain.Entities.rf_plan? plan = null;
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            plan = await _context.rf_plan
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    e => e.upgrade_group_id == groupId
                         && e.isActive == true
                         && e.isDeleted == false,
                    cancellationToken);
        }

        plan ??= await _context.rf_plan
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.isDefault == true
                     && e.isActive == true
                     && e.isDeleted == false,
                cancellationToken);

        if (plan == null)
        {
            return null;
        }

        return new EffectivePlanDto
        {
            Id = plan.id,
            PlanName = plan.plan_name,
            OrderSeq = plan.order_seq,
            MaxActiveTasks = plan.max_active_tasks,
            ReportPeriodMonths = plan.report_period_months,
            AllowsCalendar = plan.allows_calendar,
            AllowsExcelImport = plan.allows_excel_import,
            AllowsSupervisorMode = plan.allows_supervisor_mode,
        };
    }

    public async Task<EffectivePlanDto?> GetEffectivePlanWithUsageAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var plan = await GetEffectivePlanAsync(userId, cancellationToken);
        if (plan == null || string.IsNullOrWhiteSpace(userId))
        {
            return plan;
        }

        plan.ActiveTaskCount = await _unitOfWork.Tasks.CountActiveByUserAsync(userId, cancellationToken);
        return plan;
    }
}
