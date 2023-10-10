using System.Text;
using Newtonsoft.Json;
using TouristarModels.Enums;
using TouristarModels.Models;
using TouristarScheduler.Contracts;

namespace TouristarScheduler.Services;

public class EmailProcessingService : IEmailProcessingService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger _logger;
    private readonly IPublishingService _publishingService;

    public EmailProcessingService(
        IRepositoryManager repository,
        ILogger<EmailProcessingService> logger,
        IPublishingService publishingService
    )
    {
        _repository = repository;
        _logger = logger;
        _publishingService = publishingService;
    }

    public async Task<int> ProcessEmails()
    {
        var alreadyFetchedEmailIds = _repository.ProcessedEmail
            .GetProcessedEmails()
            .Select(x => x.UniqueId);
        var upcomingTripIds = _repository.Trip.FindAllUpcomingTrips().Select(x => x.Id);
        _logger.LogInformation("About to fetch relevant emails from Gmail API.");
        var emails = (
            await _repository.Gmail.GetEmailMessages(
                upcomingTripIds.ToList(),
                alreadyFetchedEmailIds.ToList()
            )
        ).ToList();
        _logger.LogInformation($"Retrieved {emails.Count} relevant emails.");

        _logger.LogInformation(
            $"About to save {emails.Count} retrieved emails to ProcessedEmail table."
        );
        _repository.ProcessedEmail.CreateManyProcessedEmails(
            emails.Select(
                x =>
                    new ProcessedEmail
                    {
                        UniqueId = x.UniqueId,
                        SentTo = x.SentTo,
                        TripId = x.TripId
                    }
            )
        );
        await _repository.Save();

        _logger.LogInformation($"About to add {emails.Count} emails to processing queue.");

        foreach (var email in emails)
        {
            var base64Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(email.Text));
            var message = new EmailProcessingMessageDto
            {
                Base64Text = base64Text,
                TripId = email.TripId
            };
            var messageJson = JsonConvert.SerializeObject(message);
            _publishingService.AddMessage(messageJson, Queues.EmailProcessingQueue);
        }

        _logger.LogInformation($"Added emails to EmailProcessingQueue.");

        return emails.Count;
    }
}
