using DeckZa.Application.DTOs.General;
using DeckZa.Application.DTOs.Notification;

namespace DeckZa.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetRecentAsync(string userId, int top = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task<BaseGeneralResponse> MarkSeenAsync(string userId, long id);
    Task<BaseGeneralResponse> MarkAllSeenAsync(string userId);
    Task CreateAsync(
        string userId,
        string shortText,
        string? longText,
        string? href,
        string typeId,
        CancellationToken cancellationToken = default);
}

public interface INotificationPusher
{
    Task PushAsync(string userId, NotificationDto notification);
}
