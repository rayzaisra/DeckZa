using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Domain.Enums;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

public class ExportJobTrackingService : IExportJobTrackingService
{
    private readonly DeckZaDbContext _context;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExportJobTrackingService> _logger;

    public ExportJobTrackingService(
        DeckZaDbContext context,
        IHostEnvironment environment,
        ILogger<ExportJobTrackingService> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    public async Task<tb_export_job> CreateAsync(
        string jobId,
        string userId,
        string exportType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var entity = new tb_export_job
        {
            job_id = jobId,
            user_id = userId,
            file_name = fileName,
            export_type = exportType,
            expires_at = now.AddDays(7),
            isActive = true,
            isDeleted = false,
            statusApproval = Const.StatusApproval.APPROVED,
            createdBy = userId,
            createdDate = now,
        };

        await _context.tb_export_job.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> CanAccessAsync(
        string jobId,
        string userId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobId) || string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var job = await _context.tb_export_job
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.job_id == jobId && e.isDeleted != true, cancellationToken);

        if (job == null)
        {
            return false;
        }

        return isAdministrator || string.Equals(job.user_id, userId, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> CanAccessByFileNameAsync(
        string fileName,
        string userId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var job = await _context.tb_export_job
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.file_name == fileName && e.isDeleted != true, cancellationToken);

        if (job == null)
        {
            return false;
        }

        return isAdministrator || string.Equals(job.user_id, userId, StringComparison.OrdinalIgnoreCase);
    }

    public Task<tb_export_job?> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default)
        => _context.tb_export_job
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.job_id == jobId && e.isDeleted != true, cancellationToken);

    public Task<tb_export_job?> GetByFileNameAsync(string fileName, CancellationToken cancellationToken = default)
        => _context.tb_export_job
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.file_name == fileName && e.isDeleted != true, cancellationToken);

    public async Task MarkReadyAsync(string jobId, string fileName, CancellationToken cancellationToken = default)
    {
        var job = await _context.tb_export_job.FirstOrDefaultAsync(
            e => e.job_id == jobId && e.isDeleted != true,
            cancellationToken);

        if (job == null)
        {
            return;
        }

        job.file_name = fileName;
        job.modifiedBy = job.user_id;
        job.modifiedDate = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var expiredJobs = await _context.tb_export_job
            .Where(e => e.isDeleted != true && e.expires_at != null && e.expires_at <= now)
            .ToListAsync(cancellationToken);

        if (expiredJobs.Count == 0)
        {
            return;
        }

        var exportsFolder = Path.Combine(_environment.ContentRootPath, "exports");
        foreach (var job in expiredJobs)
        {
            TryDeleteFile(exportsFolder, job.file_name);
            job.isDeleted = true;
            job.isActive = false;
            job.deletedBy = "system";
            job.deletedDate = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cleaned up {Count} expired export jobs.", expiredJobs.Count);
    }

    private void TryDeleteFile(string exportsFolder, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        try
        {
            var path = Path.Combine(exportsFolder, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete expired export file {FileName}", fileName);
        }
    }
}
