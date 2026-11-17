using DeckZa.Application.Interfaces;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Services;

public class ErrorLogService : IErrorLogService
{
    private readonly AuditLogDbContext _audit;
    private readonly ICurrentUserService _currentUser;

    public ErrorLogService(AuditLogDbContext audit, ICurrentUserService currentUser)
    {
        _audit = audit;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string entity, string action, Exception exception, string? userId = null)
    {
        try
        {
            var message = exception.ToString();
            if (message.Length > 4000)
            {
                message = message[..4000];
            }

            _audit.tb_log.Add(new tb_log
            {
                entity = entity,
                log_type = action,
                log_msg = message,
                created_date = DateTime.Now,
                created_by = userId ?? _currentUser.UserId ?? "system",
            });
            await _audit.SaveChangesAsync();
        }
        catch
        {
            // Error logging must never break the original request.
        }
    }
}
