namespace DeckZa.Application.Interfaces;

public interface IUserSessionNotifier
{
    Task NotifySessionRevokedAsync(string userId, string? sessionId);
}
