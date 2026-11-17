using Microsoft.EntityFrameworkCore;

using DeckZa.Application.Interfaces.Repositories;
using DeckZa.Domain.Entities;
using DeckZa.Infrastructure.Data;

namespace DeckZa.Infrastructure.Repositories.Management;

public class RfDefaultValueRepository : IRfDefaultValueRepository
{
    private readonly DeckZaDbContext _context;

    public RfDefaultValueRepository(DeckZaDbContext context) => _context = context;

    public Task<rf_default_value?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
        => _context.rf_default_value.FirstOrDefaultAsync(
            e => e.def_code == code && e.isDeleted != true,
            cancellationToken);
}
