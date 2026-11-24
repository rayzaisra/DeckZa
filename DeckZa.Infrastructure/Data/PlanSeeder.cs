using Microsoft.EntityFrameworkCore;

using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Data;

public static class PlanSeeder
{
    private static readonly (
        string Id,
        string Name,
        string Description,
        decimal Price,
        int? MaxActiveTasks,
        int? ReportPeriodMonths,
        bool AllowsCalendar,
        bool AllowsExcelImport,
        bool AllowsSupervisorMode,
        string UpgradeGroupId,
        int Order,
        string[] Features)[] Plans =
    {
        (
            Const.PlanIds.Guest,
            "Guest",
            "Akses terbatas untuk pengguna yang baru mendaftar: maksimal 50 task aktif dan report hanya periode bulan berjalan.",
            0m,
            50,
            1,
            false,
            false,
            false,
            Const.DEFAULT_GROUP_ID_GUEST_CODE,
            1,
            new[] { "Task management up to 50 task", "Report performance per bulan today" }
        ),
        (
            Const.PlanIds.Member,
            "Member",
            "Akses penuh: task tanpa batas, report periode bebas, kalender, dan import Excel.",
            49000m,
            null,
            null,
            true,
            true,
            true,
            Const.DEFAULT_GROUP_ID_MEMBER_CODE,
            2,
            new[]
            {
                "Task management unlimited",
                "Report Performance unlimited periodenya",
                "Calendar task",
                "Support supervisor mode"
            }
        ),
    };

    public static async Task SeedAsync(DeckZaDbContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        foreach (var item in Plans)
        {
            if (!await context.rf_plan.AnyAsync(e => e.id == item.Id, cancellationToken))
            {
                await context.rf_plan.AddAsync(new rf_plan
                {
                    id = item.Id,
                    plan_name = item.Name,
                    description = item.Description,
                    price_amount = item.Price,
                    price_currency = "IDR",
                    billing_period = Const.BillingPeriod.ONE_TIME,
                    billing_interval_count = 1,
                    max_active_tasks = item.MaxActiveTasks,
                    report_period_months = item.ReportPeriodMonths,
                    allows_calendar = item.AllowsCalendar,
                    allows_excel_import = item.AllowsExcelImport,
                    allows_supervisor_mode = item.AllowsSupervisorMode,
                    upgrade_group_id = item.UpgradeGroupId,
                    order_seq = item.Order,
                    statusApproval = Const.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = item.Id == Const.PlanIds.Member,
                    approvedBy = "system",
                    approvedDate = now,
                    createdBy = "system",
                    createdDate = now,
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }

            await SeedFeaturesAsync(context, item.Id, item.Features, now, cancellationToken);
        }
    }

    private static async Task SeedFeaturesAsync(
        DeckZaDbContext context,
        string planId,
        string[] features,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await context.rf_plan_feature
            .Where(e => e.plan_id == planId)
            .Select(e => e.feature_text)
            .ToHashSetAsync(cancellationToken);

        var changed = false;
        for (var index = 0; index < features.Length; index++)
        {
            if (existing.Contains(features[index]))
            {
                continue;
            }

            await context.rf_plan_feature.AddAsync(new rf_plan_feature
            {
                plan_id = planId,
                feature_text = features[index],
                order_seq = index + 1,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                approvedBy = "system",
                approvedDate = now,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);

            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
