using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Toluwaloope.LocationDetector.Api.Interfaces;

namespace Toluwaloope.LocationDetector.Api.Functions;

/// <summary>
/// Azure Function to retrieve caller information including geolocation data.
/// </summary>
public class GetCallerInformationFunction
{
    private readonly ILogger<GetCallerInformationFunction> _logger;
    private readonly IGeoLocation _geoLocation;
    private readonly TelemetryClient _telemetryClient;

    public GetCallerInformationFunction(
        ILogger<GetCallerInformationFunction> logger,
        IGeoLocation geoLocation,
        TelemetryClient telemetryClient)
    {
        _logger = logger;
        _geoLocation = geoLocation;
        _telemetryClient = telemetryClient;
    }

    [Function("GetCallerInformation")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        FunctionContext executionContext)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetOrCreateGuidCorrelationId(req);

        try
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["FunctionName"] = "GetCallerInformation",
                ["InvocationId"] = executionContext.InvocationId
            });

            SetCorrelationHeader(req, correlationId);

            var requestProperties = new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["InvocationId"] = executionContext.InvocationId,
                ["Method"] = req.Method,
                ["Path"] = req.Path
            };

            _telemetryClient.TrackEvent("GetCallerInformation.RequestStarted", requestProperties);
            _logger.LogInformation("Step 1: Processing caller information request.");

            // Log request headers
            _logger.LogInformation("Step 2: Logging request headers.");
            LogRequestHeaders(req);

            // Extract and validate client IP
            _logger.LogInformation("Step 3: Extracting client IP.");
            var ip = ExtractClientIp(req);
            if (string.IsNullOrWhiteSpace(ip))
            {
                const string errorMessage = "Unable to detect your IP address.";
                _logger.LogWarning("Step 4: Client IP extraction failed. {ErrorMessage}", errorMessage);
                _telemetryClient.TrackEvent("GetCallerInformation.MissingIp", requestProperties);
                return new BadRequestObjectResult(errorMessage);
            }

            requestProperties["ClientIp"] = ip;
            _logger.LogInformation("Step 4: Resolved client IP: {Ip}", ip);

            // Get geolocation information
            _logger.LogInformation("Step 5: Starting geolocation lookup for {Ip}.", ip);
            var location = await _geoLocation.GetGeoLocationAsync(ip, correlationId);
            if (location == null)
            {
                const string errorMessage = "Could not determine your geolocation information.";
                _logger.LogWarning("Step 6: Geolocation lookup returned no result. {ErrorMessage} IP: {Ip}", errorMessage, ip);
                _telemetryClient.TrackEvent("GetCallerInformation.GeoLookupNotFound", requestProperties);
                return new NotFoundObjectResult(errorMessage);
            }

            _logger.LogInformation("Step 6: Geolocation lookup succeeded.");
            _logger.LogInformation("Location details - City: {City}, Zip: {Zip}, Region: {Region}, Country: {Country}, Lat: {Lat}, Lon: {Lon}", location.City, location.Zip, location.RegionName, location.Country, location.Lat, location.Lon);

            requestProperties["City"] = location.City;
            requestProperties["Region"] = location.RegionName;
            requestProperties["Country"] = location.Country;
            _telemetryClient.TrackEvent("GetCallerInformation.GeoLookupSucceeded", requestProperties);
            
            var responseMessage = string.Format("Gotcha!! Toluwaloope location detector has detected your IP: {0} | Location: {1}, {2}, {3}, {4} (Lat: {5}, Long: {6})", ip, location.City, location.Zip, location.RegionName, location.Country, location.Lat, location.Lon);
            _logger.LogInformation("Step 7: Successfully resolved location for IP {Ip}", ip);

            return new OkObjectResult(responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Step Error: An error occurred while processing caller information.");
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["FunctionName"] = "GetCallerInformation",
                ["InvocationId"] = executionContext.InvocationId
            });
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
        finally
        {
            stopwatch.Stop();
            _telemetryClient.TrackEvent("GetCallerInformation.RequestCompleted", new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["FunctionName"] = "GetCallerInformation",
                ["InvocationId"] = executionContext.InvocationId
            }, new Dictionary<string, double>
            {
                ["DurationMs"] = stopwatch.Elapsed.TotalMilliseconds
            });
        }
    }

    /// <summary>
    /// Extracts the client IP address from the request, checking X-Forwarded-For header first,
    /// then falling back to the direct connection IP.
    /// </summary>
    private string? ExtractClientIp(HttpRequest req)
    {
        // Try X-Forwarded-For header (common in proxied/load-balanced scenarios)
        var ip = req.Headers["X-Forwarded-For"]
            .FirstOrDefault()?
            .Split(',')
            .FirstOrDefault()?
            .Trim();

        // Skip localhost addresses
        if (!string.IsNullOrWhiteSpace(ip) && !IsLocalhost(ip))
        {
            return ip;
        }

        // Fall back to direct connection IP
        var directIp = req.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(directIp) || IsLocalhost(directIp) ? null : directIp;
    }

    private static string GetOrCreateGuidCorrelationId(HttpRequest req)
    {
        var headerValue = req.Headers["x-correlation-id"].FirstOrDefault()
            ?? req.Headers["x-ms-client-tracking-id"].FirstOrDefault();

        if (Guid.TryParse(headerValue, out var parsedGuid))
        {
            return parsedGuid.ToString("D");
        }

        return Guid.NewGuid().ToString("D");
    }

    private static void SetCorrelationHeader(HttpRequest req, string correlationId)
    {
        req.HttpContext.Response.Headers["x-correlation-id"] = correlationId;
    }

    /// <summary>
    /// Determines if the given IP address is a localhost/loopback address.
    /// </summary>
    private static bool IsLocalhost(string? ip)
    {
        return string.IsNullOrWhiteSpace(ip) || 
               ip is "127.0.0.1" or "::1" or "localhost";
    }

    /// <summary>
    /// Logs request headers for debugging purposes.
    /// </summary>
    private void LogRequestHeaders(HttpRequest req)
    {
        if (req?.Headers?.Count > 0)
        {
            var headerBuilder = new StringBuilder();
            foreach (var (key, value) in req.Headers)
            {
                headerBuilder.AppendLine($"{key}: {value}");
            }
            _logger.LogDebug("Request Headers:\n{Headers}", headerBuilder);
        }
    }
}
