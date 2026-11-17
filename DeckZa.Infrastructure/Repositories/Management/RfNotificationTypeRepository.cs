using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class RfNotificationTypeRepository : IRfNotificationTypeRepository
{
    private readonly DeckZaDbContext _context;

    public RfNotificationTypeRepository(DeckZaDbContext context) => _context = context;

    public Task<rf_notification_type?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
        => _context.rf_notification_type.FirstOrDefaultAsync(
            e => e.id == id && e.isDeleted != true,
            cancellationToken);

    public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        => _context.rf_notification_type.AnyAsync(
            e => e.id == id && e.isDeleted != true,
            cancellationToken);

    public Task AddAsync(rf_notification_type entity, CancellationToken cancellationToken = default)
        => _context.rf_notification_type.AddAsync(entity, cancellationToken).AsTask();
}
