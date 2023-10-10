using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using TouristarConsumer.Contracts;
using TouristarConsumer.Repositories;
using TouristarModels.Contracts;
using TouristarModels.Models;
using TouristarModels.Repositories;
using TouristarScheduler.Contracts;
using TouristarScheduler.Models;

namespace TouristarScheduler.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private IGmailRepository? _gmailRepository;
    private ITripRepository? _tripRepository;
    private IProcessedEmailRepository? _processedEmailRepository;
    private ITicketRepository? _ticketRepository;
    private INotificationRepository? _notificationRepository;
    private IMessagingRepository? _messagingRepository;

    private readonly DatabaseContext _context;
    private readonly ILogger<IGmailRepository> _gmailLogger;
    private ILogger<IMessagingRepository> _messagingLogger;
    private IOptionsMonitor<EmailSyncingConfig> _emailSyncingConfig;

    public RepositoryManager(
        ILogger<IGmailRepository> gmailLogger,
        ILogger<IMessagingRepository> messagingLogger,
        DatabaseContext context,
        IOptionsMonitor<EmailSyncingConfig> emailSyncingConfig
    )
    {
        _messagingLogger = messagingLogger;
        _gmailLogger = gmailLogger;
        _context = context;
        _emailSyncingConfig = emailSyncingConfig;
    }

    public IGmailRepository Gmail
    {
        get
        {
            _gmailRepository ??= new GmailRepository(_gmailLogger, _emailSyncingConfig);
            return _gmailRepository;
        }
    }

    public ITripRepository Trip
    {
        get
        {
            _tripRepository ??= new TripRepository(_context);
            return _tripRepository;
        }
    }

    public IProcessedEmailRepository ProcessedEmail
    {
        get
        {
            _processedEmailRepository ??= new ProcessedEmailRepository(_context);
            return _processedEmailRepository;
        }
    }

    public ITicketRepository Ticket
    {
        get
        {
            _ticketRepository ??= new TicketRepository(_context);
            return _ticketRepository;
        }
    }

    public INotificationRepository Notification
    {
        get
        {
            _notificationRepository ??= new NotificationRepository(_context);
            return _notificationRepository;
        }
    }

    public IMessagingRepository Messaging
    {
        get
        {
            var googleCredential = GoogleCredential.FromFile(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_secrets.json")
            );
            _messagingRepository ??= new MessagingRepository(_messagingLogger, googleCredential);
            return _messagingRepository;
        }
    }

    async Task IRepositoryManager.Save()
    {
        await Task.Run(() => _context.SaveChangesAsync());
    }
}
