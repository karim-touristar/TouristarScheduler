using TouristarModels.Enums;

namespace TouristarScheduler.Contracts;

public interface IPublishingService
{
    void AddMessage(string message, Queues queue);
}