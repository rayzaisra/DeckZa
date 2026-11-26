using DeckZa.Domain.Entities;

namespace DeckZa.Application.Interfaces.Repositories;

public interface IUmUserAppFlagRepository
{
    Task<um_user_app_flag?> GetByUserIdAsync(string userId);
    Task AddAsync(um_user_app_flag entity);
    Task UpdateAsync(um_user_app_flag entity);

    /// <summary>
    /// Mencabut sesi aktif user: session id dikosongkan dan status login dimatikan.
    /// Mengembalikan session id yang dicabut, atau null bila tidak ada sesi aktif.
    /// </summary>
    Task<string?> RevokeSessionAsync(string userId, CancellationToken cancellationToken = default);
}
