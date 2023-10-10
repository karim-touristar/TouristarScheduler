namespace TouristarScheduler.Models;

public class CronTimes
{
    // Every 1 minute.
    public static string EmailProcessingJob => "0 0/1 * 1/1 * ? *";
    // Every 1 hours.
    public static string TicketProcessingJob => "0 0 0/1 1/1 * ? *";
    // Every 1 minute.
     public static string NotificationProcessingJob => "0 0 0/1 1/1 * ? *";
}