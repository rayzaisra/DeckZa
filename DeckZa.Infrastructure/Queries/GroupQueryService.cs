using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Group;
using DeckZa.Application.Interfaces.Queries;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Queries;

public class GroupQueryService : IGroupQueryService
{
    private readonly DeckZaDbContext _context;

    public GroupQueryService(DeckZaDbContext context) => _context = context;

    public async Task<GeneralListResponseDto<GroupListItemDto>> GetListAsync(
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);

        var query = _context.um_group_app
            .Where(g => g.isDeleted != true)
            .Select(g => new GroupListItemDto
            {
                GroupId = g.group_id,
                GroupName = g.group_name,
                UserCount = _context.um_user_group.Count(ug => ug.group_id == g.group_id && ug.isDeleted != true),
                IsActive = g.isActive == true,
                StatusApproval = g.statusApproval,
                CreatedDate = g.createdDate,
            });

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.GroupId.Contains(search)
                || (x.GroupName != null && x.GroupName.Contains(search)));
        }

        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        query = (request.SortColumn ?? string.Empty).ToLowerInvariant() switch
        {
            "groupid" => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
            "groupname" => desc ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
            "usercount" => desc ? query.OrderByDescending(x => x.UserCount) : query.OrderBy(x => x.UserCount),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => desc ? query.OrderByDescending(x => x.GroupId) : query.OrderBy(x => x.GroupId),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new GeneralListResponseDto<GroupListItemDto>
        {
            Data = data,
            TotalCount = totalCount,
        };
    }
}
