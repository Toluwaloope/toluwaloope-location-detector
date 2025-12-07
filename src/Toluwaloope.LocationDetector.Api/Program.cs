using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toluwaloope.LocationDetector.Api.Interfaces;
using Toluwaloope.LocationDetector.Api.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Register HTTP client factory
        services.AddHttpClient();
        
        // Register services
        services.AddSingleton<IGeoLocation, GeoLocationService>();
    })
    .Build();

host.Run();