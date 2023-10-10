using TouristarModels.Models;

namespace TouristarScheduler.Contracts;

public interface IGmailRepository
{
    Task<IEnumerable<EmailMessage>>
        GetEmailMessages(List<long> upcomingTripIds, List<string> fetchedEmailIds);
}