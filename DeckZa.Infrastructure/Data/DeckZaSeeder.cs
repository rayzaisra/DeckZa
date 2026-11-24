using Microsoft.EntityFrameworkCore;

using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Data;

public static class DeckZaSeeder
{
    private static readonly (string Id, string Description, int Order)[] Applications =
    {
        ("APP001", "LOS", 1),
        ("APP002", "LOS T24 Service", 2),
        ("APP003", "Web Kalkulator", 3),
        ("APP004", "HD Approval", 4),
    };

    private static readonly (string Id, string Description, int Order)[] Statuses =
    {
        ("STS001", "Draft", 1),
        ("STS002", "In Progress", 2),
        ("STS003", "Ready For UAT", 3),
        ("STS004", "Ready To Deploy", 4),
        ("STS005", "Done", 5),
    };

    private static readonly (string Id, string Description, int Order)[] TaskTypes =
    {
        ("TTP001", "Enhance", 1),
        ("TTP002", "Bug Fixing", 2),
        ("TTP003", "Daily Routine (Update Data / Report)", 3),
    };

    private static readonly (string Id, string Description, int Order)[] TaskPriorities =
    {
        ("TPR001", "Low", 1),
        ("TPR002", "Normal", 2),
        ("TPR003", "High", 3),
        ("TPR004", "VIP", 4),
    };

    private static readonly (string Unique, string Name, string? Url, string Level, string Parent, string Icon, int Order, string? Component)[] Menus =
    {
        ("TSK", "Task Workspace", null, "A", "", "Work", 2, null),
        ("TSK-KANBAN", "Task Management", "/dashboard/task-management", "D", "TSK", "Checklist", 2, "DeckZa.Shared.Pages.AsMember.Kanban, DeckZa.Shared"),
        ("TSK-CAL", "Calendar", "/dashboard/calendar", "D", "TSK", "CalendarToday", 3, "DeckZa.Shared.Pages.AsMember.Calendar, DeckZa.Shared"),
        ("TSK-RPT", "Report", "/dashboard/report", "D", "TSK", "BarChart", 4, "DeckZa.Shared.Pages.AsMember.Report, DeckZa.Shared"),
    };

    private static readonly string[] ObsoleteMenuCodes =
    {
        "MNT-DECKZA",
        "MNT-DECKZA-APP",
        "MNT-DECKZA-STA",
        "MNT-DECKZA-TYP",
        "MNT-DECKZA-PRI",
        "TSK-DASH",
    };

    /// <summary>
    /// Menu milik grup Guest. Calendar (TSK-CAL) sengaja tidak termasuk.
    /// Framework Flows (DOC-FLOW) juga tidak termasuk — dokumentasi framework
    /// hanya untuk Administrator; Guest memakai DeckZa Flows (DOC-DECKZA).
    /// </summary>
    private static readonly string[] GuestMenuCodes =
    {
        "TSK",
        "TSK-KANBAN",
        "TSK-RPT",
        "DOC",
        "DOC-DECKZA",
    };

    /// <summary>
    /// Awalan <c>menu_unique</c> untuk menu Administrator (Setting / Maintenance).
    /// Grup selain Administrator tidak boleh memetakan menu dengan awalan ini.
    /// </summary>
    private const string AdminMenuPrefix = "MNT";

    public static async Task SeedAsync(DeckZaDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedMasterDataAsync(context, cancellationToken);
        await SeedMemberGroupAsync(context, cancellationToken);
        await RemoveObsoleteMenusAsync(context, cancellationToken);
        await SeedMenusAsync(context, cancellationToken);
        await SeedGuestGroupAsync(context, cancellationToken);
    }

    private static async Task RemoveObsoleteMenusAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var menus = await context.um_menu
            .Where(e => ObsoleteMenuCodes.Contains(e.menu_unique))
            .ToListAsync(cancellationToken);

        if (menus.Count == 0)
        {
            return;
        }

        var menuIds = menus.Select(e => e.id).ToList();
        var mappings = await context.um_group_menu
            .Where(e => e.menu_id.HasValue && menuIds.Contains(e.menu_id.Value))
            .ToListAsync(cancellationToken);

        context.um_group_menu.RemoveRange(mappings);
        context.um_menu.RemoveRange(menus);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMasterDataAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        var existingApplications = await context.rf_application
            .Select(e => e.application_id)
            .ToHashSetAsync(cancellationToken);
        foreach (var item in Applications)
        {
            if (existingApplications.Contains(item.Id))
            {
                continue;
            }

            await context.rf_application.AddAsync(new rf_application
            {
                application_id = item.Id,
                description = item.Description,
                order_seq = item.Order,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "admin",
                createdDate = now,
            }, cancellationToken);
        }

        var existingStatuses = await context.rf_status
            .Select(e => e.status_id)
            .ToHashSetAsync(cancellationToken);
        foreach (var item in Statuses)
        {
            if (existingStatuses.Contains(item.Id))
            {
                continue;
            }

            await context.rf_status.AddAsync(new rf_status
            {
                status_id = item.Id,
                description = item.Description,
                order_seq = item.Order,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "admin",
                createdDate = now,
            }, cancellationToken);
        }

        var existingTaskTypes = await context.rf_task_type
            .Select(e => e.task_type_id)
            .ToHashSetAsync(cancellationToken);
        foreach (var item in TaskTypes)
        {
            if (existingTaskTypes.Contains(item.Id))
            {
                continue;
            }

            await context.rf_task_type.AddAsync(new rf_task_type
            {
                task_type_id = item.Id,
                description = item.Description,
                order_seq = item.Order,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "admin",
                createdDate = now,
            }, cancellationToken);
        }

        var existingPriorities = await context.rf_task_priority
            .Select(e => e.task_priority_id)
            .ToHashSetAsync(cancellationToken);
        foreach (var item in TaskPriorities)
        {
            if (existingPriorities.Contains(item.Id))
            {
                continue;
            }

            await context.rf_task_priority.AddAsync(new rf_task_priority
            {
                task_priority_id = item.Id,
                description = item.Description,
                order_seq = item.Order,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "admin",
                createdDate = now,
            }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMemberGroupAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var exists = await context.um_group_app
            .AnyAsync(e => e.group_id == Const.DEFAULT_GROUP_ID_MEMBER_CODE, cancellationToken);
        if (exists)
        {
            return;
        }

        await context.um_group_app.AddAsync(new um_group_app
        {
            group_id = Const.DEFAULT_GROUP_ID_MEMBER_CODE,
            group_name = "Member",
            statusApproval = Const.StatusApproval.APPROVED,
            isActive = true,
            isDeleted = false,
            isLocked = false,
            isDefault = false,
            createdBy = "system",
            createdDate = DateTime.Now,
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMenusAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        foreach (var item in Menus)
        {
            var existing = await context.um_menu
                .FirstOrDefaultAsync(e => e.menu_unique == item.Unique, cancellationToken);

            if (existing == null)
            {
                existing = new um_menu
                {
                    menu_unique = item.Unique,
                    menu_name = item.Name,
                    menu_url = item.Url,
                    parent_level_id = item.Level,
                    menu_unique_parent = item.Parent,
                    menu_icon = item.Icon,
                    order_seq = item.Order,
                    component_type = item.Component,
                    statusApproval = Const.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = false,
                    createdBy = "system",
                    createdDate = now,
                };

                await context.um_menu.AddAsync(existing, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            var groupId = item.Unique.StartsWith("MNT", StringComparison.Ordinal)
                ? Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE
                : Const.DEFAULT_GROUP_ID_MEMBER_CODE;

            var mapped = await context.um_group_menu.AnyAsync(
                e => e.group_id == groupId && e.menu_id == existing.id,
                cancellationToken);

            if (!mapped)
            {
                await context.um_group_menu.AddAsync(new um_group_menu
                {
                    group_id = groupId,
                    menu_id = existing.id,
                    statusApproval = Const.StatusApproval.APPROVED,
                    isActive = true,
                    isDeleted = false,
                    isLocked = false,
                    isDefault = false,
                    createdBy = "system",
                    createdDate = now,
                }, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static async Task SeedGuestGroupAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        if (!await context.um_group_app.AnyAsync(e => e.group_id == Const.DEFAULT_GROUP_ID_GUEST_CODE, cancellationToken))
        {
            await context.um_group_app.AddAsync(new um_group_app
            {
                group_id = Const.DEFAULT_GROUP_ID_GUEST_CODE,
                group_name = "Guest",
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        // Menu milik Administrator (MNT*) tidak boleh dipetakan ke Guest —
        // sama seperti grup Member yang memang tidak memetakan Setting.
        await RemoveAdminMenuMappingsAsync(context, cancellationToken);

        var menus = await context.um_menu
            .Where(e => GuestMenuCodes.Contains(e.menu_unique)
                        || (e.parent_level_id == Const.ParentLevel.FIRST_PARENT
                            && (e.menu_unique_parent == null || e.menu_unique_parent == string.Empty)))
            .Where(e => e.menu_unique == null || !e.menu_unique.StartsWith(AdminMenuPrefix))
            .ToListAsync(cancellationToken);

        if (menus.Count == 0)
        {
            return;
        }

        var menuIds = menus.Select(e => e.id).ToList();
        var mapped = await context.um_group_menu
            .Where(e => e.group_id == Const.DEFAULT_GROUP_ID_GUEST_CODE
                        && e.menu_id.HasValue
                        && menuIds.Contains(e.menu_id.Value))
            .Select(e => e.menu_id!.Value)
            .ToListAsync(cancellationToken);

        var mappedIds = mapped.ToHashSet();
        var changed = false;

        foreach (var menu in menus)
        {
            if (mappedIds.Contains(menu.id))
            {
                continue;
            }

            await context.um_group_menu.AddAsync(new um_group_menu
            {
                group_id = Const.DEFAULT_GROUP_ID_GUEST_CODE,
                menu_id = menu.id,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
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

    /// <summary>
    /// Melepas pemetaan menu Administrator (MNT*) dari grup Guest.
    /// Menu "Setting" tidak boleh muncul di sidebar Guest — sama seperti grup Member.
    /// </summary>
    private static async Task RemoveAdminMenuMappingsAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var adminMenuIds = await context.um_menu
            .Where(e => e.menu_unique != null && e.menu_unique.StartsWith(AdminMenuPrefix))
            .Select(e => e.id)
            .ToListAsync(cancellationToken);

        if (adminMenuIds.Count == 0)
        {
            return;
        }

        var mappings = await context.um_group_menu
            .Where(e => e.group_id == Const.DEFAULT_GROUP_ID_GUEST_CODE
                        && e.menu_id.HasValue
                        && adminMenuIds.Contains(e.menu_id.Value))
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0)
        {
            return;
        }

        context.um_group_menu.RemoveRange(mappings);
        await context.SaveChangesAsync(cancellationToken);
    }
}
