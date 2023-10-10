using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ReadSharp;
using TouristarModels.Models;
using TouristarScheduler.Contracts;
using TouristarScheduler.Helpers;
using TouristarScheduler.Models;

namespace TouristarScheduler.Repositories;

public class GmailRepository : IGmailRepository
{
    private readonly ILogger _logger;
    private readonly EmailSyncingConfig _config;

    public GmailRepository(ILogger logger, IOptionsMonitor<EmailSyncingConfig> config)
    {
        _logger = logger;
        _config = config.CurrentValue;
    }

    private ServiceAccountCredential GetGoogleCredential()
    {
        _logger.LogInformation("Attempting to retrieve Google credential.");
        var pathJson = "client_secrets.json";
        var json = File.ReadAllText(pathJson);
        var serviceAccount = JsonConvert.DeserializeObject<ServiceAccountDto>(json);
        if (serviceAccount == null)
        {
            throw new Exception(
                "Could not find service account data for Google credential generation."
            );
        }

        var serviceAccountCredentialInitialiser = new ServiceAccountCredential.Initializer(
            serviceAccount.ClientEmail
        )
        {
            User = _config.DelegationEmail,
            Scopes = new[] { GmailService.Scope.GmailReadonly }
        }.FromPrivateKey(serviceAccount.PrivateKey);
        var credential = new ServiceAccountCredential(serviceAccountCredentialInitialiser);
        if (!credential.RequestAccessTokenAsync(CancellationToken.None).Result)
        {
            throw new InvalidOperationException("Gmail access token failed.");
        }

        return credential;
    }

    private GmailService GetGmailService() =>
        new(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = GetGoogleCredential(),
                ApplicationName = "touristarbackend"
            }
        );

    public async Task<IEnumerable<EmailMessage>> GetEmailMessages(
        List<long> upcomingTripIds,
        List<string> fetchedEmailIds
    )
    {
        var messages = new List<EmailMessage>();
        var service = GetGmailService();

        // Retrieve email unprocessed email messages.
        var fetchedMessages = await service.Users.Messages
            .List(_config.DelegationEmail)
            .ExecuteAsync();
        if (!fetchedMessages.Messages.Any())
        {
            _logger.LogInformation("No messages were found in inbox.");
            return messages;
        }
        var emails = fetchedMessages.Messages.Where(m => !fetchedEmailIds.Contains(m.Id)).ToList();
        if (emails.Count == 0)
        {
            _logger.LogInformation($"Found {emails.Count} to process.");
            return new List<EmailMessage>();
        }

        foreach (var email in emails)
        {
            var message = await service.Users.Messages
                .Get(_config.DelegationEmail, email.Id)
                .ExecuteAsync();
            var sentTo = GetDeliveredToValue(message.Payload.Headers);
            if (sentTo == null)
            {
                continue;
            }

            if (!sentTo.StartsWith(_config.Email.Split("@").First()))
            {
                continue;
            }

            var tripId = GetTripIdFromEmail(sentTo);
            if (tripId < 0)
            {
                continue;
            }

            if (!upcomingTripIds.Contains(tripId))
            {
                continue;
            }

            var part = SelectAppropriatePart(message.Payload.Parts);
            var data = Convert.FromBase64String(Base64Helper.RepairBase64String(part.Body.Data));
            string decodedEmailBody = Encoding.UTF8.GetString(data);
            if (IsHtmlPart(part) == true)
            {
                _logger.LogInformation(
                    "Detected text/html type email part. Decoding HTML into readable string."
                );
                decodedEmailBody = HtmlUtilities.ConvertToPlainText(decodedEmailBody);
            }

            messages.Add(
                new()
                {
                    UniqueId = email.Id,
                    Text = decodedEmailBody,
                    SentTo = sentTo,
                    TripId = tripId,
                }
            );
        }

        return messages;
    }

    private static MessagePart SelectAppropriatePart(IList<MessagePart> parts)
    {
        var textPart = parts.FirstOrDefault(p => p.MimeType == "text/plain");
        return textPart ?? parts.First();
    }

    private static bool? IsHtmlPart(MessagePart part) => part.MimeType == "text/html";

    private static string? GetDeliveredToValue(IEnumerable<MessagePartHeader> headers)
    {
        return headers.FirstOrDefault(h => h.Name == "Delivered-To")?.Value;
    }

    private static long GetTripIdFromEmail(string emailTo)
    {
        try
        {
            var id = emailTo.Split("+").Last().Split("@").First();
            return (long)Convert.ToUInt64(id);
        }
        catch
        {
            return -1;
        }
    }
}
