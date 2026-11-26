using Microsoft.EntityFrameworkCore;

using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.MasterData;
using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

public class MasterDataService : IMasterDataService
{
    private static readonly HashSet<string> SupportedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        "rf_application",
        "rf_task_type",
        "rf_task_priority",
        "rf_status",
    };

    private readonly DeckZaDbContext _context;
    private readonly IPlanService _planService;

    public MasterDataService(DeckZaDbContext context, IPlanService planService)
    {
        _context = context;
        _planService = planService;
    }

    public async Task<GeneralListResponseDto<MasterItemDto>> GetListAsync(
        string table,
        string userId,
        DataTableRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var name = Normalize(table);
        if (name == null)
        {
            return new GeneralListResponseDto<MasterItemDto>();
        }

        var page = Math.Max(1, request.PageNumber);
        var pageSize = request.PageSize <= 0 ? 10 : Math.Min(request.PageSize, 200);
        var search = (request.SearchTerm ?? string.Empty).Trim();
        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return name switch
        {
            "rf_application" => await PagedApplicationsAsync(userId, search, page, pageSize, request.SortColumn, desc, cancellationToken),
            "rf_task_type" => await PagedTaskTypesAsync(userId, search, page, pageSize, request.SortColumn, desc, cancellationToken),
            "rf_task_priority" => await PagedPrioritiesAsync(userId, search, page, pageSize, request.SortColumn, desc, cancellationToken),
            _ => await PagedStatusesAsync(userId, search, page, pageSize, request.SortColumn, desc, cancellationToken),
        };
    }

    public async Task<List<MasterItemDto>> GetAllAsync(
        string table,
        string userId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var name = Normalize(table);
        if (name == null)
        {
            return new List<MasterItemDto>();
        }

        var term = (search ?? string.Empty).Trim();

        return name switch
        {
            "rf_application" => await FilterApplications(userId, term)
                .OrderBy(e => e.order_seq).ThenBy(e => e.description)
                .Select(e => new MasterItemDto
                {
                    Id = e.application_id,
                    Description = e.description,
                    OrderSeq = e.order_seq,
                    IsActive = e.isActive == true,
                    StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
                })
                .ToListAsync(cancellationToken),
            "rf_task_type" => await FilterTaskTypes(userId, term)
                .OrderBy(e => e.order_seq).ThenBy(e => e.description)
                .Select(e => new MasterItemDto
                {
                    Id = e.task_type_id,
                    Description = e.description,
                    OrderSeq = e.order_seq,
                    IsActive = e.isActive == true,
                    StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
                })
                .ToListAsync(cancellationToken),
            "rf_task_priority" => await FilterPriorities(userId, term)
                .OrderBy(e => e.order_seq).ThenBy(e => e.description)
                .Select(e => new MasterItemDto
                {
                    Id = e.task_priority_id,
                    Description = e.description,
                    OrderSeq = e.order_seq,
                    IsActive = e.isActive == true,
                    StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
                })
                .ToListAsync(cancellationToken),
            _ => await FilterStatuses(userId, term)
                .OrderBy(e => e.order_seq).ThenBy(e => e.description)
                .Select(e => new MasterItemDto
                {
                    Id = e.status_id,
                    Description = e.description,
                    OrderSeq = e.order_seq,
                    IsActive = e.isActive == true,
                    IsInitialStatus = e.is_initial_status,
                    IsCompletedStatus = e.is_completed_status,
                    StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
                })
                .ToListAsync(cancellationToken),
        };
    }

    public async Task<BaseGeneralResponse> SaveAsync(string table, MasterItemDto item, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Fail("User session is not valid.");
        }

        var name = Normalize(table);
        if (name == null)
        {
            return FailKey("UnsupportedMasterTable", "Unsupported master table.");
        }

        var description = (item.Description ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(description))
        {
            return FailKey("NameRequired", "Description is required.");
        }

        var id = (item.Id ?? string.Empty).Trim();
        var isNew = string.IsNullOrWhiteSpace(id);
        var now = DateTime.Now;

        var plan = await _planService.GetEffectivePlanAsync(userId, cancellationToken);
        var maxRows = plan?.MaxMasterRowsPerTable;

        try
        {
            return name switch
            {
                "rf_application" => await SaveApplicationAsync(id, isNew, description, item, userId, now, maxRows, cancellationToken),
                "rf_task_type" => await SaveTaskTypeAsync(id, isNew, description, item, userId, now, maxRows, cancellationToken),
                "rf_task_priority" => await SavePriorityAsync(id, isNew, description, item, userId, now, maxRows, cancellationToken),
                _ => await SaveStatusAsync(id, isNew, description, item, userId, now, maxRows, cancellationToken),
            };
        }
        catch (DbUpdateException)
        {
            return FailKey("MasterSaveConflict", "This master data could not be saved. The name may already exist.");
        }
    }

    public async Task<BaseGeneralResponse> DeleteAsync(string table, string id, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Fail("User session is not valid.");
        }

        id = (id ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            return Fail("Id is required.");
        }

        var name = Normalize(table);
        if (name == null)
        {
            return FailKey("UnsupportedMasterTable", "Unsupported master table.");
        }

        try
        {
            return name switch
            {
                "rf_application" => await DeactivateApplicationAsync(id, userId, cancellationToken),
                "rf_task_type" => await DeactivateTaskTypeAsync(id, userId, cancellationToken),
                "rf_task_priority" => await DeactivatePriorityAsync(id, userId, cancellationToken),
                _ => await DeactivateStatusAsync(id, userId, cancellationToken),
            };
        }
        catch (DbUpdateException)
        {
            return FailKey("MasterDeactivateFailed", "This master data could not be deactivated.");
        }
    }

    private async Task<GeneralListResponseDto<MasterItemDto>> PagedApplicationsAsync(
        string userId, string search, int page, int pageSize, string? sortColumn, bool desc, CancellationToken ct)
    {
        var query = FilterApplications(userId, search);
        var total = await query.CountAsync(ct);

        query = (sortColumn ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "description" or "name" => desc ? query.OrderByDescending(e => e.description) : query.OrderBy(e => e.description),
            "orderseq" or "order" => desc ? query.OrderByDescending(e => e.order_seq) : query.OrderBy(e => e.order_seq),
            "isactive" or "active" => desc ? query.OrderByDescending(e => e.isActive) : query.OrderBy(e => e.isActive),
            "statusapproval" => desc ? query.OrderByDescending(e => e.statusApproval) : query.OrderBy(e => e.statusApproval),
            _ => query.OrderBy(e => e.order_seq).ThenBy(e => e.description),
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new MasterItemDto
            {
                Id = e.application_id,
                Description = e.description,
                OrderSeq = e.order_seq,
                IsActive = e.isActive == true,
                StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
            })
            .ToListAsync(ct);

        return new GeneralListResponseDto<MasterItemDto> { Data = rows, TotalCount = total };
    }

    private async Task<GeneralListResponseDto<MasterItemDto>> PagedTaskTypesAsync(
        string userId, string search, int page, int pageSize, string? sortColumn, bool desc, CancellationToken ct)
    {
        var query = FilterTaskTypes(userId, search);
        var total = await query.CountAsync(ct);

        query = (sortColumn ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "description" or "name" => desc ? query.OrderByDescending(e => e.description) : query.OrderBy(e => e.description),
            "orderseq" or "order" => desc ? query.OrderByDescending(e => e.order_seq) : query.OrderBy(e => e.order_seq),
            "isactive" or "active" => desc ? query.OrderByDescending(e => e.isActive) : query.OrderBy(e => e.isActive),
            "statusapproval" => desc ? query.OrderByDescending(e => e.statusApproval) : query.OrderBy(e => e.statusApproval),
            _ => query.OrderBy(e => e.order_seq).ThenBy(e => e.description),
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new MasterItemDto
            {
                Id = e.task_type_id,
                Description = e.description,
                OrderSeq = e.order_seq,
                IsActive = e.isActive == true,
                StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
            })
            .ToListAsync(ct);

        return new GeneralListResponseDto<MasterItemDto> { Data = rows, TotalCount = total };
    }

    private async Task<GeneralListResponseDto<MasterItemDto>> PagedPrioritiesAsync(
        string userId, string search, int page, int pageSize, string? sortColumn, bool desc, CancellationToken ct)
    {
        var query = FilterPriorities(userId, search);
        var total = await query.CountAsync(ct);

        query = (sortColumn ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "description" or "name" => desc ? query.OrderByDescending(e => e.description) : query.OrderBy(e => e.description),
            "orderseq" or "order" => desc ? query.OrderByDescending(e => e.order_seq) : query.OrderBy(e => e.order_seq),
            "isactive" or "active" => desc ? query.OrderByDescending(e => e.isActive) : query.OrderBy(e => e.isActive),
            "statusapproval" => desc ? query.OrderByDescending(e => e.statusApproval) : query.OrderBy(e => e.statusApproval),
            _ => query.OrderBy(e => e.order_seq).ThenBy(e => e.description),
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new MasterItemDto
            {
                Id = e.task_priority_id,
                Description = e.description,
                OrderSeq = e.order_seq,
                IsActive = e.isActive == true,
                StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
            })
            .ToListAsync(ct);

        return new GeneralListResponseDto<MasterItemDto> { Data = rows, TotalCount = total };
    }

    private async Task<GeneralListResponseDto<MasterItemDto>> PagedStatusesAsync(
        string userId, string search, int page, int pageSize, string? sortColumn, bool desc, CancellationToken ct)
    {
        var query = FilterStatuses(userId, search);
        var total = await query.CountAsync(ct);

        query = (sortColumn ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "description" or "name" => desc ? query.OrderByDescending(e => e.description) : query.OrderBy(e => e.description),
            "orderseq" or "order" => desc ? query.OrderByDescending(e => e.order_seq) : query.OrderBy(e => e.order_seq),
            "isactive" or "active" => desc ? query.OrderByDescending(e => e.isActive) : query.OrderBy(e => e.isActive),
            "statusapproval" => desc ? query.OrderByDescending(e => e.statusApproval) : query.OrderBy(e => e.statusApproval),
            _ => query.OrderBy(e => e.order_seq).ThenBy(e => e.description),
        };

        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new MasterItemDto
            {
                Id = e.status_id,
                Description = e.description,
                OrderSeq = e.order_seq,
                IsActive = e.isActive == true,
                IsInitialStatus = e.is_initial_status,
                IsCompletedStatus = e.is_completed_status,
                StatusApproval = e.statusApproval ?? Const.StatusApproval.DRAFT,
            })
            .ToListAsync(ct);

        return new GeneralListResponseDto<MasterItemDto> { Data = rows, TotalCount = total };
    }

    private IQueryable<rf_application> FilterApplications(string userId, string search)
    {
        var query = _context.rf_application.AsNoTracking()
            .Where(e => e.owner_user_id == userId && e.isDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.description.Contains(search) || e.application_id.Contains(search));
        }

        return query;
    }

    private IQueryable<rf_task_type> FilterTaskTypes(string userId, string search)
    {
        var query = _context.rf_task_type.AsNoTracking()
            .Where(e => e.owner_user_id == userId && e.isDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.description.Contains(search) || e.task_type_id.Contains(search));
        }

        return query;
    }

    private IQueryable<rf_task_priority> FilterPriorities(string userId, string search)
    {
        var query = _context.rf_task_priority.AsNoTracking()
            .Where(e => e.owner_user_id == userId && e.isDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.description.Contains(search) || e.task_priority_id.Contains(search));
        }

        return query;
    }

    private IQueryable<rf_status> FilterStatuses(string userId, string search)
    {
        var query = _context.rf_status.AsNoTracking()
            .Where(e => e.owner_user_id == userId && e.isDeleted == false);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.description.Contains(search) || e.status_id.Contains(search));
        }

        return query;
    }

    private async Task<BaseGeneralResponse> SaveApplicationAsync(string id, bool isNew, string description, MasterItemDto item, string userId, DateTime now, int? maxRows, CancellationToken ct)
    {
        var dup = await _context.rf_application.AnyAsync(e => e.owner_user_id == userId && e.description == description && e.application_id != id && e.isDeleted == false, ct);
        if (dup) return FailKey("ApplicationNameExists", "Application name already exists.");

        rf_application entity;
        if (isNew)
        {
            if (await IsQuotaReachedAsync(_context.rf_application.Where(e => e.owner_user_id == userId && e.isActive == true && e.isDeleted == false), maxRows, ct))
                return QuotaFail("applications", maxRows);

            entity = new rf_application
            {
                application_id = NewId(),
                owner_user_id = userId,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
            };
            await _context.rf_application.AddAsync(entity, ct);
        }
        else
        {
            var existing = await _context.rf_application.FirstOrDefaultAsync(e => e.application_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
            if (existing == null) return FailKey("ApplicationNotFound", "Application not found.");
            entity = existing;
            entity.modifiedBy = userId;
            entity.modifiedDate = now;
        }

        entity.description = description;
        entity.order_seq = item.OrderSeq;
        entity.isActive = item.IsActive;
        entity.statusApproval = Const.StatusApproval.APPROVED;
        entity.approvedBy = userId;
        entity.approvedDate = now;
        await _context.SaveChangesAsync(ct);
        return Ok(isNew ? "Application created." : "Application updated.");
    }

    private async Task<BaseGeneralResponse> SaveTaskTypeAsync(string id, bool isNew, string description, MasterItemDto item, string userId, DateTime now, int? maxRows, CancellationToken ct)
    {
        var dup = await _context.rf_task_type.AnyAsync(e => e.owner_user_id == userId && e.description == description && e.task_type_id != id && e.isDeleted == false, ct);
        if (dup) return FailKey("TaskTypeNameExists", "Task type name already exists.");

        rf_task_type entity;
        if (isNew)
        {
            if (await IsQuotaReachedAsync(_context.rf_task_type.Where(e => e.owner_user_id == userId && e.isActive == true && e.isDeleted == false), maxRows, ct))
                return QuotaFail("task types", maxRows);

            entity = new rf_task_type
            {
                task_type_id = NewId(),
                owner_user_id = userId,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
            };
            await _context.rf_task_type.AddAsync(entity, ct);
        }
        else
        {
            var existing = await _context.rf_task_type.FirstOrDefaultAsync(e => e.task_type_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
            if (existing == null) return FailKey("TaskTypeNotFound", "Task type not found.");
            entity = existing;
            entity.modifiedBy = userId;
            entity.modifiedDate = now;
        }

        entity.description = description;
        entity.order_seq = item.OrderSeq;
        entity.isActive = item.IsActive;
        entity.statusApproval = Const.StatusApproval.APPROVED;
        entity.approvedBy = userId;
        entity.approvedDate = now;
        await _context.SaveChangesAsync(ct);
        return Ok(isNew ? "Task type created." : "Task type updated.");
    }

    private async Task<BaseGeneralResponse> SavePriorityAsync(string id, bool isNew, string description, MasterItemDto item, string userId, DateTime now, int? maxRows, CancellationToken ct)
    {
        var dup = await _context.rf_task_priority.AnyAsync(e => e.owner_user_id == userId && e.description == description && e.task_priority_id != id && e.isDeleted == false, ct);
        if (dup) return FailKey("PriorityNameExists", "Priority name already exists.");

        rf_task_priority entity;
        if (isNew)
        {
            if (await IsQuotaReachedAsync(_context.rf_task_priority.Where(e => e.owner_user_id == userId && e.isActive == true && e.isDeleted == false), maxRows, ct))
                return QuotaFail("priorities", maxRows);

            entity = new rf_task_priority
            {
                task_priority_id = NewId(),
                owner_user_id = userId,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
            };
            await _context.rf_task_priority.AddAsync(entity, ct);
        }
        else
        {
            var existing = await _context.rf_task_priority.FirstOrDefaultAsync(e => e.task_priority_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
            if (existing == null) return FailKey("PriorityNotFound", "Priority not found.");
            entity = existing;
            entity.modifiedBy = userId;
            entity.modifiedDate = now;
        }

        entity.description = description;
        entity.order_seq = item.OrderSeq;
        entity.isActive = item.IsActive;
        entity.statusApproval = Const.StatusApproval.APPROVED;
        entity.approvedBy = userId;
        entity.approvedDate = now;
        await _context.SaveChangesAsync(ct);
        return Ok(isNew ? "Priority created." : "Priority updated.");
    }

    private async Task<BaseGeneralResponse> SaveStatusAsync(string id, bool isNew, string description, MasterItemDto item, string userId, DateTime now, int? maxRows, CancellationToken ct)
    {
        var dup = await _context.rf_status.AnyAsync(e => e.owner_user_id == userId && e.description == description && e.status_id != id && e.isDeleted == false, ct);
        if (dup) return FailKey("StatusNameExists", "Status name already exists.");

        rf_status entity;
        if (isNew)
        {
            if (await IsQuotaReachedAsync(_context.rf_status.Where(e => e.owner_user_id == userId && e.isActive == true && e.isDeleted == false), maxRows, ct))
                return QuotaFail("statuses", maxRows);

            entity = new rf_status
            {
                status_id = NewId(),
                owner_user_id = userId,
                isDeleted = false,
                isLocked = false,
                isDefault = false,
                createdBy = userId,
                createdDate = now,
            };
            await _context.rf_status.AddAsync(entity, ct);
        }
        else
        {
            var existing = await _context.rf_status.FirstOrDefaultAsync(e => e.status_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
            if (existing == null) return FailKey("StatusNotFound", "Status not found.");
            entity = existing;
            entity.modifiedBy = userId;
            entity.modifiedDate = now;
        }

        var targetIsInitial = item.IsInitialStatus;
        var targetIsCompleted = item.IsCompletedStatus;

        if (!item.IsActive && (targetIsInitial || targetIsCompleted))
        {
            return FailKey("DefaultStatusMustRemain", "A default or completed status must remain active.");
        }

        if (targetIsInitial)
        {
            var others = await _context.rf_status
                .Where(e => e.owner_user_id == userId && e.isDeleted == false && e.status_id != entity.status_id && e.is_initial_status)
                .ToListAsync(ct);
            foreach (var other in others)
            {
                other.is_initial_status = false;
                other.modifiedBy = userId;
                other.modifiedDate = now;
            }
        }

        if (targetIsCompleted)
        {
            var others = await _context.rf_status
                .Where(e => e.owner_user_id == userId && e.isDeleted == false && e.status_id != entity.status_id && e.is_completed_status)
                .ToListAsync(ct);
            foreach (var other in others)
            {
                other.is_completed_status = false;
                other.modifiedBy = userId;
                other.modifiedDate = now;
            }
        }

        if (!isNew)
        {
            if (entity.is_initial_status && !targetIsInitial)
            {
                var hasAnotherInitial = await _context.rf_status.AnyAsync(e => e.owner_user_id == userId && e.isDeleted == false && e.isActive == true && e.status_id != entity.status_id && e.is_initial_status, ct);
                if (!hasAnotherInitial) return FailKey("DefaultStatusRequired", "At least one active default status is required.");
            }

            if (entity.is_completed_status && !targetIsCompleted)
            {
                var hasAnotherCompleted = await _context.rf_status.AnyAsync(e => e.owner_user_id == userId && e.isDeleted == false && e.isActive == true && e.status_id != entity.status_id && e.is_completed_status, ct);
                if (!hasAnotherCompleted)
                {
                    var hasActiveTasks = await _context.tb_task.AnyAsync(t => t.createdBy == userId && t.isActive == true && t.isDeleted == false, ct);
                    if (hasActiveTasks) return FailKey("CompletedStatusInUse", "Cannot unset the only completed status while active tasks exist.");
                }
            }
        }

        entity.description = description;
        entity.order_seq = item.OrderSeq;
        entity.isActive = item.IsActive;
        entity.is_initial_status = targetIsInitial;
        entity.is_completed_status = targetIsCompleted;
        entity.statusApproval = Const.StatusApproval.APPROVED;
        entity.approvedBy = userId;
        entity.approvedDate = now;
        await _context.SaveChangesAsync(ct);
        return Ok(isNew ? "Status created." : "Status updated.");
    }

    private async Task<BaseGeneralResponse> DeactivateApplicationAsync(string id, string userId, CancellationToken ct)
    {
        var entity = await _context.rf_application.FirstOrDefaultAsync(e => e.application_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
        if (entity == null) return FailKey("ApplicationNotFound", "Application not found.");

        var inUse = await _context.tb_task.AnyAsync(t => t.createdBy == userId && t.application_id == id && t.isActive == true && t.isDeleted == false, ct);
        ApplyRemoval(entity, userId, inUse);
        await _context.SaveChangesAsync(ct);
        return Ok(inUse
            ? "Application deactivated because it is used by tasks. Remove it from those tasks first."
            : "Application deleted.");
    }

    private async Task<BaseGeneralResponse> DeactivateTaskTypeAsync(string id, string userId, CancellationToken ct)
    {
        var entity = await _context.rf_task_type.FirstOrDefaultAsync(e => e.task_type_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
        if (entity == null) return FailKey("TaskTypeNotFound", "Task type not found.");

        var inUse = await _context.tb_task.AnyAsync(t => t.createdBy == userId && t.task_type_id == id && t.isActive == true && t.isDeleted == false, ct);
        ApplyRemoval(entity, userId, inUse);
        await _context.SaveChangesAsync(ct);
        return Ok(inUse
            ? "Task type deactivated because it is used by tasks. Remove it from those tasks first."
            : "Task type deleted.");
    }

    private async Task<BaseGeneralResponse> DeactivatePriorityAsync(string id, string userId, CancellationToken ct)
    {
        var entity = await _context.rf_task_priority.FirstOrDefaultAsync(e => e.task_priority_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
        if (entity == null) return FailKey("PriorityNotFound", "Priority not found.");

        var inUse = await _context.tb_task.AnyAsync(t => t.createdBy == userId && t.task_priority_id == id && t.isActive == true && t.isDeleted == false, ct);
        ApplyRemoval(entity, userId, inUse);
        await _context.SaveChangesAsync(ct);
        return Ok(inUse
            ? "Priority deactivated because it is used by tasks. Remove it from those tasks first."
            : "Priority deleted.");
    }

    private async Task<BaseGeneralResponse> DeactivateStatusAsync(string id, string userId, CancellationToken ct)
    {
        var entity = await _context.rf_status.FirstOrDefaultAsync(e => e.status_id == id && e.owner_user_id == userId && e.isDeleted == false, ct);
        if (entity == null) return FailKey("StatusNotFound", "Status not found.");
        if (entity.is_initial_status || entity.is_completed_status)
            return FailKey("ProtectedStatusCannotDelete", "Default or Completed status cannot be deleted. Assign another status first.");

        var inUse = await _context.tb_task.AnyAsync(t => t.createdBy == userId && t.status_id == id && t.isActive == true && t.isDeleted == false, ct);
        ApplyRemoval(entity, userId, inUse);
        await _context.SaveChangesAsync(ct);
        return Ok(inUse
            ? "Status deactivated because it is used by tasks. Move those tasks to another status first."
            : "Status deleted.");
    }

    /// <summary>
    /// Menghapus master data memakai aturan soft delete:
    /// <c>status_approval = 8</c>, <c>is_active = 0</c>, <c>is_deleted = 1</c>,
    /// <c>deleted_by</c> dan <c>deleted_date</c> terisi.
    /// Baris yang masih dipakai task aktif hanya dinonaktifkan (<c>is_active = 0</c>)
    /// agar referensi task tidak putus.
    /// </summary>
    private static void ApplyRemoval(BaseEntity entity, string userId, bool inUse)
    {
        var now = DateTime.Now;

        if (inUse)
        {
            entity.isActive = false;
            entity.modifiedBy = userId;
            entity.modifiedDate = now;
            return;
        }

        entity.statusApproval = Const.StatusApproval.DELETED;
        entity.isActive = false;
        entity.isDeleted = true;
        entity.deletedBy = userId;
        entity.deletedDate = now;
        entity.modifiedBy = userId;
        entity.modifiedDate = now;
    }

    private static string NewId()
        => "MST-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    private static async Task<bool> IsQuotaReachedAsync<T>(IQueryable<T> activeQuery, int? maxRows, CancellationToken ct)
    {
        if (!maxRows.HasValue) return false;
        var count = await activeQuery.CountAsync(ct);
        return count >= maxRows.Value;
    }

    private static BaseGeneralResponse QuotaFail(string label, int? maxRows)
        => new()
        {
            Code = Const.ERROR_CODE,
            Success = false,
            Message = $"You have reached the limit of {maxRows} active {label} for your plan. Upgrade to add more.",
            MessageKey = "MasterQuotaReached",
            MessageArgs = new[] { maxRows?.ToString() ?? string.Empty, label },
        };

    private static BaseGeneralResponse Fail(string message)
        => new() { Code = Const.ERROR_CODE, Success = false, Message = message };

    private static BaseGeneralResponse FailKey(string messageKey, string message)
        => new() { Code = Const.ERROR_CODE, Success = false, Message = message, MessageKey = messageKey };

    private static BaseGeneralResponse Ok(string message)
        => new() { Code = Const.SUCCESS_CODE, Success = true, Message = message };

    private static string? Normalize(string table)
    {
        var name = (table ?? string.Empty).Trim().ToLowerInvariant();
        return SupportedTables.Contains(name) ? name : null;
    }
}
