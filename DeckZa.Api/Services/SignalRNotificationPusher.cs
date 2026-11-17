using Microsoft.AspNetCore.SignalR;

using DeckZa.Application.DTOs.Notification;
using DeckZa.Application.Interfaces;
using DeckZa.Api.Hubs;

namespace DeckZa.Api.Services;

public class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<UserHub> _hub;

    public SignalRNotificationPusher(IHubContext<UserHub> hub) => _hub = hub;

    public Task PushAsync(string userId, NotificationDto notification)
        => _hub.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
}
