using Quartz;
using TouristarScheduler.Contracts;

namespace TouristarScheduler.Jobs;

public class TicketProcessingJob : IJob
{
    private readonly ILogger _logger;
    private readonly ITicketProcessingService _ticketProcessingService;

    public TicketProcessingJob(ILogger<TicketProcessingJob> logger, ITicketProcessingService ticketProcessingService)
    {
        _logger = logger;
        _ticketProcessingService = ticketProcessingService;
    }

    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting TicketProcessingJob execution.");
            var count = _ticketProcessingService.ProcessTickets();
            _logger.LogInformation(
                $"Completed TicketProcessingJob job execution. Processed {count} tickets.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"There was an error executing TicketProcessingJob. {exception}");
        }

        return Task.CompletedTask;
    }
}