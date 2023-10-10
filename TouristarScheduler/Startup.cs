using Microsoft.EntityFrameworkCore;
using Npgsql;
using Quartz;
using TouristarModels.Enums;
using TouristarModels.Models;
using TouristarScheduler.Contracts;
using TouristarScheduler.Jobs;
using TouristarScheduler.Models;
using TouristarScheduler.Repositories;
using TouristarScheduler.Services;

namespace TouristarScheduler;

public class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        SetupConfiguration(services, configuration);
        AddDatabaseContext(services, configuration);
        ConfigureQuartz(services, configuration);
        AddScopedServices(services);
    }

    private static void ConfigureQuartz(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuartzOptions>(configuration.GetSection("Quartz"));

        services.Configure<QuartzOptions>(options =>
        {
            options.Scheduling.IgnoreDuplicates = true;
            options.Scheduling.OverWriteExistingData = true;
        });

        services.AddQuartz(q =>
        {
            q.SchedulerId = "Scheduler-Core";
            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });
            q.ScheduleJob<EmailProcessingJob>(
                trigger =>
                    trigger
                        .WithIdentity("EmailProcessingJobTrigger")
                        .WithCronSchedule(CronTimes.EmailProcessingJob)
            );
            q.ScheduleJob<TicketProcessingJob>(
                trigger =>
                    trigger
                        .WithIdentity("TicketProcessingJobTrigger")
                        .WithCronSchedule(CronTimes.TicketProcessingJob)
            );
            q.ScheduleJob<NotificationProcessingJob>(
                trigger =>
                    trigger
                        .WithIdentity("NotificationProcessingJobTrigger")
                        .WithCronSchedule(CronTimes.NotificationProcessingJob)
            );
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
    }

    private static void AddScopedServices(IServiceCollection services)
    {
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IEmailProcessingService, EmailProcessingService>();
        services.AddScoped<ITicketProcessingService, TicketProcessingService>();
        services.AddScoped<IPublishingService, PublishingService>();
    }

    private static void AddDatabaseContext(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionConfig = configuration
            .GetSection("ConnectionStrings")
            .Get<ConnectionConfig>();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionConfig?.DbConnection);
        dataSourceBuilder.MapEnum<ActivityType>();
        dataSourceBuilder.MapEnum<TicketLeg>();
        dataSourceBuilder.MapEnum<TripDocumentType>();
        var dataSource = dataSourceBuilder.Build();
        services.AddDbContextPool<DatabaseContext>(options =>
        {
            options.UseNpgsql(dataSource);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }

    private static void SetupConfiguration(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<ConnectionConfig>(configuration.GetSection("ConnectionStrings"));
        services.Configure<EmailSyncingConfig>(configuration.GetSection("EmailSyncing"));
    }
}
