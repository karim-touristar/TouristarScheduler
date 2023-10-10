using Newtonsoft.Json;
using TouristarModels.Enums;
using TouristarModels.Models;
using TouristarScheduler.Contracts;

namespace TouristarScheduler.Services;

public class TicketProcessingService : ITicketProcessingService
{
    private readonly IRepositoryManager _repository;
    private readonly ILogger _logger;
    private readonly IPublishingService _publishingService;

    public TicketProcessingService(IRepositoryManager repository, ILogger<TicketProcessingService> logger,
        IPublishingService publishingService)
    {
        _repository = repository;
        _logger = logger;
        _publishingService = publishingService;
    }

    public int ProcessTickets()
    {
        // Find tickets with estimated departure within 2 days from now.
        _logger.LogInformation("About to fetch tickets suitable for processing.");
        IEnumerable<Ticket> tickets = _repository.Ticket.FindTicketsWithDepartureWithinDays(2).ToList();

        if (!tickets.Any())
        {
            _logger.LogInformation("No tickets were found with suitable departure date.");
            return 0;
        }

        var skippedTickets = 0;
        foreach (var ticket in tickets)
        {
            if (ticket.FlightStatus != null)
            {
                _logger.LogInformation(
                    $"Skipping processing for ticket id: {ticket.Id} as it already has a FlightStatus record in the db.");
                skippedTickets++;
                continue;
            }

            var message = new FlightStatusMessageDto()
            {
                TicketId = ticket.Id
            };
            var messageJson = JsonConvert.SerializeObject(message);
            _publishingService.AddMessage(messageJson, Queues.FlightStatusQueue);
            _logger.LogInformation($"Added ticket with id: {ticket.Id} to FlightStatusQueue.");
        }

        return tickets.Count() - skippedTickets;
    }
}