using TouristarConsumer.Contracts;
using TouristarModels.Contracts;

namespace TouristarScheduler.Contracts;

public interface IRepositoryManager
{
    IGmailRepository Gmail { get; }
    ITripRepository Trip { get; }
    IProcessedEmailRepository ProcessedEmail { get; }
    ITicketRepository Ticket { get; }
    INotificationRepository Notification { get; }
    IMessagingRepository Messaging { get; }
    Task Save();
}
