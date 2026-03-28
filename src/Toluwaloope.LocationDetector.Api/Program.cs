using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toluwaloope.LocationDetector.Api.Interfaces;
using Toluwaloope.LocationDetector.Api.Services;

var appInsightsConnectionString =
    Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")
    ?? Environment.GetEnvironmentVariable("APPINSIGHTS_CONNECTIONSTRING");

if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
{
    // Normalize common variable names so WorkerService telemetry can pick it up.
    Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING", appInsightsConnectionString);
}
else
{
    Console.WriteLine("[Startup Warning] APPLICATIONINSIGHTS_CONNECTION_STRING is not set. ILogger traces will not be sent to Application Insights.");
}

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Remove the default AI logger filter so Information logs are sent.
        services.Configure<LoggerFilterOptions>(options =>
        {
            var defaultRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }

            options.MinLevel = LogLevel.Information;
            options.Rules.Add(new LoggerFilterRule(null, "Function.GetCallerInformation.User", LogLevel.Information, null));
            options.Rules.Add(new LoggerFilterRule(null, "Toluwaloope.LocationDetector.Api", LogLevel.Information, null));
        });
        
        // Register HTTP client factory
        services.AddHttpClient();
        
        // Register services
        services.AddSingleton<IGeoLocation, GeoLocationService>();
    })
    .Build();

host.Run();