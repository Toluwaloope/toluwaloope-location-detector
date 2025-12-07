using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddSingleton<Toluwaloope.LocationDetector.Api.Interfaces.IGeoLocation, Toluwaloope.LocationDetector.Api.GeoLocation>();
    })
    .Build();

host.Run();