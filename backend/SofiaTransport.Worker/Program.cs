using Quartz;
using SofiaTransport.Infrastructure.DependencyInjection;
using SofiaTransport.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddHttpClient();

builder.Services.AddQuartz(q =>
{
    var delayJobKey = new JobKey("DelayAggregationJob");
    q.AddJob<DelayAggregationJob>(opts => opts.WithIdentity(delayJobKey));
    q.AddTrigger(opts => opts
        .ForJob(delayJobKey)
        .WithCronSchedule("0 0 * * * ?"));

    var mlJobKey = new JobKey("MlRetrainTriggerJob");
    q.AddJob<MlRetrainTriggerJob>(opts => opts.WithIdentity(mlJobKey));
    q.AddTrigger(opts => opts
        .ForJob(mlJobKey)
        .WithCronSchedule("0 0 2 * * ?"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddHostedService<GtfsPollingService>();

var host = builder.Build();
host.Run();
