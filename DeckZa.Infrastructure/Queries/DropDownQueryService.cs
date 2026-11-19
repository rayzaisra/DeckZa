using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.Interfaces;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class DropDownQueryService : IDropDownService
{
    private readonly DeckZaDbContext _context;

    public DropDownQueryService(DeckZaDbContext context) => _context = context;

    public Task<List<DropdownItemDto>> GetGroupsAsync()
        => _context.um_group_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .OrderBy(e => e.group_name)
            .Select(e => new DropdownItemDto { Value = e.group_id, Text = e.group_name ?? e.group_id })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetGendersAsync()
        => _context.enum_gender
            .OrderBy(e => e.gender_code)
            .Select(e => new DropdownItemDto { Value = e.gender_code, Text = e.gender_name ?? e.gender_code })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetUsersAsync()
        => _context.um_user_app
            .Where(e => e.isActive != false && e.isDeleted != true)
            .OrderBy(e => e.user_id)
            .Select(e => new DropdownItemDto { Value = e.user_id, Text = e.full_name ?? e.user_id })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetMenuIconsAsync()
        => _context.enum_menu_icon
            .OrderBy(e => e.menu_icon_code)
            .Select(e => new DropdownItemDto
            {
                Value = e.menu_icon_code,
                Text = e.menu_icon_name ?? e.menu_icon_code,
            })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetMenuParentLevelsAsync()
        => _context.enum_menu_parent_level
            .OrderBy(e => e.parent_level_code)
            .Select(e => new DropdownItemDto
            {
                Value = e.parent_level_code,
                Text = e.parent_level_name ?? e.parent_level_code,
            })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetTaskApplicationsAsync()
        => _context.rf_application
            .Where(e => e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .Select(e => new DropdownItemDto { Value = e.application_id, Text = e.description })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetTaskTypesAsync()
        => _context.rf_task_type
            .Where(e => e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .Select(e => new DropdownItemDto { Value = e.task_type_id, Text = e.description })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetTaskPrioritiesAsync()
        => _context.rf_task_priority
            .Where(e => e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .Select(e => new DropdownItemDto { Value = e.task_priority_id, Text = e.description })
            .ToListAsync();

    public Task<List<DropdownItemDto>> GetTaskStatusesAsync()
        => _context.rf_status
            .Where(e => e.isActive == true && e.isDeleted == false)
            .OrderBy(e => e.order_seq)
            .ThenBy(e => e.description)
            .Select(e => new DropdownItemDto { Value = e.status_id, Text = e.description })
            .ToListAsync();
}
