namespace TouristarScheduler.Contracts;

public interface IEmailProcessingService
{
    Task<int> ProcessEmails();
}