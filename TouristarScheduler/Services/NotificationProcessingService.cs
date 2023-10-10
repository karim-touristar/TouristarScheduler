using TouristarModels.Contracts;
using TouristarScheduler.Contracts;

namespace TouristarScheduler.Services;

class NotificationProcessingService : INotificationProcessingService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger<INotificationProcessingService> _logger;

    public NotificationProcessingService(
        IRepositoryManager repository,
        ILogger<INotificationProcessingService> logger
    )
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<int> FetchAndSendNotifications()
    {
        int count = 0;
        var notifications = _repository.Notification.GetNotificationsToSend();

        foreach (var notification in notifications)
        {
            if (notification.User.DeviceToken == null)
            {
                _logger.LogWarning(
                    $"Could not find device id for user id: {notification.User.Id}. Skipping notification processing. Notification id: {notification.Id}"
                );
                continue;
            }
            await _repository.Messaging.SendPushNotification(
                notification.User.DeviceToken,
                notification.Title,
                notification.Body,
                null
            );
            var notificationToUpdate = _repository.Notification.FindById(notification.Id);
            notificationToUpdate.SentAt = DateTime.Now;
            _repository.Notification.UpdateNotification(notificationToUpdate);
            await _repository.Save();
            count++;
        }
        return count;
    }
}
