using Microsoft.EntityFrameworkCore;

using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;

namespace DeckZa.Infrastructure.Data;

public static class MenuSeeder
{
    private const string DocumentationUnique = "DOC";
    private const string DocumentationPageUnique = "DOC-FLOW";
    private const string DocumentationPageRoute = "/dashboard/documentation/framework-flows";
    private const string DocumentationComponent = "DeckZa.Shared.Pages.Documentation.FrameworkFlows, DeckZa.Shared";

    private const string DeckZaPageUnique = "DOC-DECKZA";
    private const string DeckZaPageRoute = "/dashboard/documentation/deckza-flows";
    private const string DeckZaComponent = "DeckZa.Shared.Pages.Documentation.DeckZaFlows, DeckZa.Shared";

    private const string PaymentRootUnique = "MNT-PAY";
    private const string PaymentListUnique = "MNT-PAY-LST";
    private const string PaymentApprovalUnique = "MNT-PAY-APR";
    private const string PaymentListRoute = "/dashboard/maintenance/payment-list";
    private const string PaymentApprovalRoute = "/dashboard/maintenance/payment-approval";
    private const string PaymentListComponent = "DeckZa.Shared.Pages.Maintenance.PaymentList, DeckZa.Shared";
    private const string PaymentApprovalComponent = "DeckZa.Shared.Pages.Maintenance.PaymentApproval, DeckZa.Shared";

    private const string TaskParameterRootUnique = "TSKP";
    private const string ApplicationUnique = "TSKP-APP";
    private const string TaskTypeUnique = "TSKP-TYP";
    private const string PriorityUnique = "TSKP-PRI";
    private const string StatusUnique = "TSKP-STA";
    private const string ApplicationRoute = "/dashboard/task-parameter/application";
    private const string TaskTypeRoute = "/dashboard/task-parameter/task-type";
    private const string PriorityRoute = "/dashboard/task-parameter/priority";
    private const string StatusRoute = "/dashboard/task-parameter/status";
    private const string MasterListComponent = "DeckZa.Shared.Pages.AsMember.TaskParameter, DeckZa.Shared";

    private static readonly (string Code, string Name)[] Icons =
    {
        ("Settings", "Settings"),
        ("EmojiPeople", "Emoji People"),
        ("Groups", "Groups"),
        ("Hail", "Hail"),
        ("MenuBook", "Menu Book"),
        ("AdminPanelSettings", "Admin Panel Settings"),
        ("FormatListBulleted", "Format List Bulleted"),
        ("Checklist", "Checklist"),
        ("AddModerator", "Add Moderator"),
        ("GppGood", "GPP Good"),
        ("Flag", "Flag"),
        ("PinDrop", "Pin Drop"),
        ("Science", "Science"),
        ("Email", "Email"),
        ("Payments", "Payments"),
    };

    private static readonly (string Unique, string Name, string? Url, string Level, string Parent, string Icon, int Order, string? Component)[] Menus =
    {
        ("MNT", "Setting", null, "A", "", "Settings", 1, null),

        ("MNT-USER", "User Maintenance", null, "B", "MNT", "EmojiPeople", 1, null),
        ("MNT-GRP", "Group Maintenance", null, "B", "MNT", "Groups", 2, null),
        ("MNT-ROLE", "Role Maintenance", null, "B", "MNT", "Hail", 3, null),
        ("MNT-MENU", "Menu Maintenance", null, "B", "MNT", "MenuBook", 4, null),
        ("MNT-PARAM", "Parameter Maintenance", null, "B", "MNT", "AdminPanelSettings", 5, null),
        ("MNT-DEMO", "Demo Screens", null, "B", "MNT", "Science", 6, null),

        ("MNT-USER-LST", "List", "/dashboard/maintenance/user-list", "D", "MNT-USER", "FormatListBulleted", 1, "DeckZa.Shared.Pages.Maintenance.UserList, DeckZa.Shared"),
        ("MNT-USER-APR", "Approval", "/dashboard/maintenance/user-approval", "D", "MNT-USER", "Checklist", 2, "DeckZa.Shared.Pages.Maintenance.UserApproval, DeckZa.Shared"),
        ("MNT-USER-FLAG", "User Flag", "/dashboard/maintenance/user-flag", "D", "MNT-USER", "Flag", 3, "DeckZa.Shared.Pages.Maintenance.UserFlag, DeckZa.Shared"),

        ("MNT-GRP-LST", "List", "/dashboard/maintenance/group-list", "D", "MNT-GRP", "FormatListBulleted", 1, "DeckZa.Shared.Pages.Maintenance.GroupList, DeckZa.Shared"),
        ("MNT-GRP-APR", "Approval", "/dashboard/maintenance/group-approval", "D", "MNT-GRP", "Checklist", 2, "DeckZa.Shared.Pages.Maintenance.GroupApproval, DeckZa.Shared"),

        ("MNT-ROLE-LST", "List", "/dashboard/maintenance/role-list", "D", "MNT-ROLE", "FormatListBulleted", 1, "DeckZa.Shared.Pages.Maintenance.RoleList, DeckZa.Shared"),
        ("MNT-ROLE-APR", "Approval", "/dashboard/maintenance/role-approval", "D", "MNT-ROLE", "Checklist", 2, "DeckZa.Shared.Pages.Maintenance.RoleApproval, DeckZa.Shared"),

        ("MNT-MENU-LST", "List", "/dashboard/maintenance/menu-list", "D", "MNT-MENU", "FormatListBulleted", 1, "DeckZa.Shared.Pages.Maintenance.MenuList, DeckZa.Shared"),
        ("MNT-MENU-APR", "Approval", "/dashboard/maintenance/menu-approval", "D", "MNT-MENU", "Checklist", 2, "DeckZa.Shared.Pages.Maintenance.MenuApproval, DeckZa.Shared"),
        ("MNT-MENU-MAP", "Mapping Menu", "/dashboard/maintenance/menu-map", "D", "MNT-MENU", "PinDrop", 3, "DeckZa.Shared.Pages.Maintenance.MenuMap, DeckZa.Shared"),

        ("MNT-PARAM-MKR", "Maker", "/dashboard/maintenance/parameter-system/maker", "D", "MNT-PARAM", "AddModerator", 1, "DeckZa.Shared.Pages.Maintenance.Parameter.ParameterSystem, DeckZa.Shared"),
        ("MNT-PARAM-APR", "Approval", "/dashboard/maintenance/parameter-system/approval", "D", "MNT-PARAM", "GppGood", 2, "DeckZa.Shared.Pages.Maintenance.Parameter.ParameterSystem, DeckZa.Shared"),

        ("MNT-DEMO-VAL", "Default Value", "SM-DEFAULT-VALUE", "D", "MNT-DEMO", "FormatListBulleted", 1, "DynamicScreen"),
        ("MNT-DEMO-URL", "URL Demo", "SM-URL-DEMO", "D", "MNT-DEMO", "PinDrop", 2, "DynamicScreen"),
    };

    public static async Task SeedAsync(DeckZaDbContext context, CancellationToken cancellationToken = default)
    {
        await EnsureDocumentationMenuAsync(context, cancellationToken);

        if (await context.um_menu.AnyAsync(e => e.menu_unique == "MNT", cancellationToken))
        {
            await RemoveEmailMenusAsync(context, cancellationToken);
            return;
        }

        foreach (var icon in Icons)
        {
            if (!await context.enum_menu_icon.AnyAsync(e => e.menu_icon_code == icon.Code, cancellationToken))
            {
                await context.enum_menu_icon.AddAsync(new enum_menu_icon
                {
                    menu_icon_code = icon.Code,
                    menu_icon_name = icon.Name,
                    menu_icon_name_alternate = null,
                }, cancellationToken);
            }
        }

        var now = DateTime.Now;
        var menuEntities = new Dictionary<string, um_menu>();

        foreach (var item in Menus)
        {
            var entity = new um_menu
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

            menuEntities[item.Unique] = entity;
            await context.um_menu.AddAsync(entity, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var menu in menuEntities.Values)
        {
            await context.um_group_menu.AddAsync(new um_group_menu
            {
                group_id = Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE,
                menu_id = menu.id,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task EnsureDocumentationMenuAsync(
        DeckZaDbContext context,
        CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var root = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = DocumentationUnique,
                menu_name = "Docs",
                parent_level_id = Const.ParentLevel.FIRST_PARENT,
                menu_unique_parent = string.Empty,
                menu_icon = "MenuBook",
                order_seq = 90,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        var frameworkPage = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = DocumentationPageUnique,
                menu_name = "Framework Flows",
                menu_url = DocumentationPageRoute,
                parent_level_id = Const.ParentLevel.CHILD,
                menu_unique_parent = DocumentationUnique,
                menu_icon = "Description",
                order_seq = 1,
                component_type = DocumentationComponent,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        var deckZaPage = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = DeckZaPageUnique,
                menu_name = "DeckZa Flows",
                menu_url = DeckZaPageRoute,
                parent_level_id = Const.ParentLevel.CHILD,
                menu_unique_parent = DocumentationUnique,
                menu_icon = "AutoStories",
                order_seq = 2,
                component_type = DeckZaComponent,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        // Aturan akses dokumentasi:
        //   - Docs (root)         -> semua grup aktif, agar dropdown muncul untuk semua peran
        //   - DeckZa Flows        -> semua grup aktif (Administrator, Member, Guest)
        //   - Framework Flows     -> HANYA Administrator (grup 00)
        var groupIds = await context.um_group_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .Select(e => e.group_id)
            .ToListAsync(cancellationToken);

        var allGroupIds = groupIds
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToList();

        var adminGroupIds = allGroupIds
            .Where(e => string.Equals(e, Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE, StringComparison.Ordinal))
            .ToList();

        await EnsureGroupMenuAsync(context, root.id, allGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, deckZaPage.id, allGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, frameworkPage.id, adminGroupIds, now, cancellationToken);

        // DB existing sudah terlanjur memetakan Framework Flows ke semua grup.
        // Cabut pemetaannya agar Member dan Guest tidak lagi melihat menu ini.
        await RemoveGroupMenuAsync(context, frameworkPage.id, allGroupIds.Except(adminGroupIds), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Menu Task Parameter untuk semua grup yang di-mapping. Dipanggil terpisah
    /// dari <see cref="SeedAsync"/> karena seeding menu utama berhenti begitu
    /// <c>MNT</c> sudah ada di DB existing.
    /// </summary>
    public static async Task EnsureTaskParameterMenusAsync(
        DeckZaDbContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        foreach (var icon in new[] { "Tune", "Apps", "Category", "Flag", "LinearScale" })
        {
            if (!await context.enum_menu_icon.AnyAsync(e => e.menu_icon_code == icon, cancellationToken))
            {
                await context.enum_menu_icon.AddAsync(new enum_menu_icon
                {
                    menu_icon_code = icon,
                    menu_icon_name = icon,
                    menu_icon_name_alternate = null,
                }, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        // Root Task Parameter ada di level A dengan order 1; Task Workspace digeser ke 2.
        await SyncWorkspaceOrderAsync(context, cancellationToken);

        var root = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = TaskParameterRootUnique,
                menu_name = "Task Parameter",
                menu_url = null,
                parent_level_id = Const.ParentLevel.FIRST_PARENT,
                menu_unique_parent = string.Empty,
                menu_icon = "Tune",
                order_seq = 1,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        var application = await EnsureMenuAsync(
            context,
            BuildChildMenu(ApplicationUnique, "Application", ApplicationRoute, "Apps", 1, now),
            cancellationToken);

        var taskType = await EnsureMenuAsync(
            context,
            BuildChildMenu(TaskTypeUnique, "Task Type", TaskTypeRoute, "Category", 2, now),
            cancellationToken);

        var priority = await EnsureMenuAsync(
            context,
            BuildChildMenu(PriorityUnique, "Priority", PriorityRoute, "Flag", 3, now),
            cancellationToken);

        var status = await EnsureMenuAsync(
            context,
            BuildChildMenu(StatusUnique, "Status", StatusRoute, "LinearScale", 4, now),
            cancellationToken);

        // Visibilitas mengikuti mapping menu, bukan hardcode grup. Default seed memetakan
        // ke semua grup aktif; admin dapat mempersempit lewat Menu Mapping.
        var groupIds = await context.um_group_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .Select(e => e.group_id)
            .ToListAsync(cancellationToken);

        var visibleGroupIds = groupIds
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToList();

        await EnsureGroupMenuAsync(context, root.id, visibleGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, application.id, visibleGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, taskType.id, visibleGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, priority.id, visibleGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, status.id, visibleGroupIds, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static um_menu BuildChildMenu(
        string unique,
        string name,
        string route,
        string icon,
        int order,
        DateTime now)
        => new()
        {
            menu_unique = unique,
            menu_name = name,
            menu_url = route,
            parent_level_id = Const.ParentLevel.CHILD,
            menu_unique_parent = TaskParameterRootUnique,
            menu_icon = icon,
            order_seq = order,
            component_type = MasterListComponent,
            statusApproval = Const.StatusApproval.APPROVED,
            isActive = true,
            isDeleted = false,
            isLocked = false,
            isDefault = false,
            createdBy = "system",
            createdDate = now,
        };

    private static async Task SyncWorkspaceOrderAsync(DeckZaDbContext context, CancellationToken cancellationToken)
    {
        var workspace = await context.um_menu
            .FirstOrDefaultAsync(e => e.menu_unique == "TSK", cancellationToken);

        if (workspace == null || workspace.order_seq == 2)
        {
            return;
        }

        workspace.order_seq = 2;
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Menu Payment untuk Administrator. Dipanggil terpisah dari <see cref="SeedAsync"/>
    /// karena seeding menu utama berhenti begitu <c>MNT</c> sudah ada.
    /// </summary>
    public static async Task EnsurePaymentMenusAsync(
        DeckZaDbContext context,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;

        // Ikon harus ada di master enum_menu_icon — seeding icon utama hanya jalan
        // saat tabel menu masih kosong.
        if (!await context.enum_menu_icon.AnyAsync(e => e.menu_icon_code == "Payments", cancellationToken))
        {
            await context.enum_menu_icon.AddAsync(new enum_menu_icon
            {
                menu_icon_code = "Payments",
                menu_icon_name = "Payments",
                menu_icon_name_alternate = null,
            }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        var root = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = PaymentRootUnique,
                menu_name = "Payment",
                menu_url = null,
                parent_level_id = Const.ParentLevel.FIRST_PARENT,
                menu_unique_parent = string.Empty,
                menu_icon = "Payments",
                order_seq = 2,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        var list = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = PaymentListUnique,
                menu_name = "List",
                menu_url = PaymentListRoute,
                parent_level_id = Const.ParentLevel.CHILD,
                menu_unique_parent = PaymentRootUnique,
                menu_icon = "FormatListBulleted",
                order_seq = 1,
                component_type = PaymentListComponent,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        var approval = await EnsureMenuAsync(
            context,
            new um_menu
            {
                menu_unique = PaymentApprovalUnique,
                menu_name = "Approval",
                menu_url = PaymentApprovalRoute,
                parent_level_id = Const.ParentLevel.CHILD,
                menu_unique_parent = PaymentRootUnique,
                menu_icon = "Checklist",
                order_seq = 2,
                component_type = PaymentApprovalComponent,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            },
            cancellationToken);

        // Payment hanya untuk Administrator.
        var adminGroupIds = new[] { Const.DEFAULT_GROUP_ID_ADMINISTRATOR_CODE };

        await EnsureGroupMenuAsync(context, root.id, adminGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, list.id, adminGroupIds, now, cancellationToken);
        await EnsureGroupMenuAsync(context, approval.id, adminGroupIds, now, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Membuat menu bila belum ada, atau memperbaiki nama dan atribut tampilannya
    /// bila sudah ada. Idempoten.
    /// </summary>
    private static async Task<um_menu> EnsureMenuAsync(
        DeckZaDbContext context,
        um_menu desired,
        CancellationToken cancellationToken)
    {
        var existing = await context.um_menu
            .FirstOrDefaultAsync(e => e.menu_unique == desired.menu_unique, cancellationToken);

        if (existing == null)
        {
            await context.um_menu.AddAsync(desired, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return desired;
        }

        var changed = false;

        if (!string.Equals(existing.menu_name, desired.menu_name, StringComparison.Ordinal))
        {
            existing.menu_name = desired.menu_name;
            changed = true;
        }

        if (!string.Equals(existing.menu_url, desired.menu_url, StringComparison.Ordinal))
        {
            existing.menu_url = desired.menu_url;
            changed = true;
        }

        if (!string.Equals(existing.component_type, desired.component_type, StringComparison.Ordinal))
        {
            existing.component_type = desired.component_type;
            changed = true;
        }

        if (!string.Equals(existing.parent_level_id, desired.parent_level_id, StringComparison.Ordinal))
        {
            existing.parent_level_id = desired.parent_level_id;
            changed = true;
        }

        if (!string.Equals(existing.menu_unique_parent ?? string.Empty, desired.menu_unique_parent ?? string.Empty, StringComparison.Ordinal))
        {
            existing.menu_unique_parent = desired.menu_unique_parent;
            changed = true;
        }

        if (!string.Equals(existing.menu_icon, desired.menu_icon, StringComparison.Ordinal))
        {
            existing.menu_icon = desired.menu_icon;
            changed = true;
        }

        if (existing.order_seq != desired.order_seq)
        {
            existing.order_seq = desired.order_seq;
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return existing;
    }

    private static async Task EnsureGroupMenuAsync(
        DeckZaDbContext context,
        long menuId,
        IEnumerable<string> groupIds,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existing = await context.um_group_menu
            .Where(e => e.menu_id == menuId)
            .Select(e => e.group_id)
            .ToListAsync(cancellationToken);

        var mapped = existing
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var groupId in groupIds)
        {
            if (mapped.Contains(groupId))
            {
                continue;
            }

            await context.um_group_menu.AddAsync(new um_group_menu
            {
                group_id = groupId,
                menu_id = menuId,
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);
        }
    }

    private static async Task RemoveGroupMenuAsync(
        DeckZaDbContext context,
        long menuId,
        IEnumerable<string> groupIds,
        CancellationToken cancellationToken)
    {
        var targets = groupIds
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!)
            .ToHashSet(StringComparer.Ordinal);

        if (targets.Count == 0)
        {
            return;
        }

        var mappings = await context.um_group_menu
            .Where(e => e.menu_id == menuId && e.group_id != null && targets.Contains(e.group_id))
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0)
        {
            return;
        }

        context.um_group_menu.RemoveRange(mappings);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task RemoveEmailMenusAsync(
        DeckZaDbContext context,
        CancellationToken cancellationToken)
    {
        var root = await context.um_menu
            .FirstOrDefaultAsync(e => e.menu_unique == "MNT-EMAIL", cancellationToken);
        if (root == null)
        {
            return;
        }

        var menuIds = new List<long> { root.id };
        var parentCodes = new List<string> { "MNT-EMAIL" };

        while (parentCodes.Count > 0)
        {
            var children = await context.um_menu
                .Where(e => e.menu_unique_parent != null && parentCodes.Contains(e.menu_unique_parent))
                .ToListAsync(cancellationToken);
            if (children.Count == 0)
            {
                break;
            }

            menuIds.AddRange(children.Select(e => e.id));
            parentCodes = children
                .Where(e => !string.IsNullOrWhiteSpace(e.menu_unique))
                .Select(e => e.menu_unique!)
                .ToList();
        }

        var mappings = await context.um_group_menu
            .Where(e => e.menu_id.HasValue && menuIds.Contains(e.menu_id.Value))
            .ToListAsync(cancellationToken);
        context.um_group_menu.RemoveRange(mappings);

        var menus = await context.um_menu
            .Where(e => menuIds.Contains(e.id))
            .ToListAsync(cancellationToken);
        context.um_menu.RemoveRange(menus);

        await context.SaveChangesAsync(cancellationToken);
    }
}
