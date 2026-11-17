using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface IRfDefaultValueRepository
{
    Task<rf_default_value?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
