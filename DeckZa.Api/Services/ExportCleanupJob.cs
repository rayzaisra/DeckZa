using DeckZa.Application.Interfaces;

namespace DeckZa.Api.Services;

public class ExportCleanupJob
{
    private readonly IExportJobTrackingService _trackingService;
    private readonly IErrorLogService _errorLog;
    private readonly ILogger<ExportCleanupJob> _logger;

    public ExportCleanupJob(
        IExportJobTrackingService trackingService,
        IErrorLogService errorLog,
        ILogger<ExportCleanupJob> logger)
    {
        _trackingService = trackingService;
        _errorLog = errorLog;
        _logger = logger;
    }

    public async Task CleanupExpiredAsync()
    {
        try
        {
            await _trackingService.CleanupExpiredAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export cleanup job failed");
            await _errorLog.LogAsync("ExportCleanup", "ERROR", ex, "system");
        }
    }
}
