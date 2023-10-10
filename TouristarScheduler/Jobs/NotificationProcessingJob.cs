using Quartz;
using TouristarModels.Contracts;

namespace TouristarScheduler.Jobs;

public class NotificationProcessingJob : IJob
{
    private readonly ILogger<NotificationProcessingJob> _logger;
    private readonly INotificationProcessingService _service;

    public NotificationProcessingJob(
        INotificationProcessingService service,
        ILogger<NotificationProcessingJob> logger
    )
    {
        _service = service;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting NotificationProcessingJob execution.");
            var count = await _service.FetchAndSendNotifications();
            _logger.LogInformation(
                $"Completed NotificationProcessingJob job execution. Processed {count} notifications."
            );
        }
        catch (Exception e)
        {
            _logger.LogError($"There was an error executing NotificationProcessingJob. {e}");
        }
    }
}
