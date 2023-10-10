using Quartz;
using TouristarScheduler.Contracts;

namespace TouristarScheduler.Jobs;

public class EmailProcessingJob : IJob
{
    private readonly ILogger _logger;
    private readonly IEmailProcessingService _service;

    public EmailProcessingJob(ILogger<EmailProcessingJob> logger, IEmailProcessingService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting EmailProcessingJob execution.");
            var count = await _service.ProcessEmails();
            _logger.LogInformation(
                $"Completed EmailProcessingJob job execution. Processed {count} emails.");
        }
        catch (Exception exception)
        {
            _logger.LogError($"There was an error executing EmailProcessingJob. {exception}");
        }
    }
}