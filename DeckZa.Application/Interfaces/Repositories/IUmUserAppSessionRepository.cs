using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface IUmUserAppSessionRepository
{
    Task UpsertAsync(string userId, string? sessionId);
    Task RemoveAsync(string userId);
}
