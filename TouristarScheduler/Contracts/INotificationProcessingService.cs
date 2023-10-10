namespace TouristarModels.Contracts;

public interface INotificationProcessingService
{
    Task<int> FetchAndSendNotifications();
}
