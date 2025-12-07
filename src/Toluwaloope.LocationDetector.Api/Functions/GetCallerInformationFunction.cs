using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
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

    public GetCallerInformationFunction(ILogger<GetCallerInformationFunction> logger, IGeoLocation geoLocation)
    {
        _logger = logger;
        _geoLocation = geoLocation;
    }

    [Function("GetCallerInformation")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("Processing caller information request.");

            // Log request headers
            LogRequestHeaders(req);

            // Extract and validate client IP
            var ip = ExtractClientIp(req);
            if (string.IsNullOrWhiteSpace(ip))
            {
                const string errorMessage = "Unable to detect your IP address.";
                _logger.LogWarning(errorMessage);
                return new BadRequestObjectResult(errorMessage);
            }

            _logger.LogInformation("Resolved client IP: {Ip}", ip);

            // Get geolocation information
            var location = await _geoLocation.GetGeoLocationAsync(ip);
            if (location == null)
            {
                const string errorMessage = "Could not determine your geolocation information.";
                _logger.LogWarning("{ErrorMessage} IP: {Ip}", errorMessage, ip);
                return new NotFoundObjectResult(errorMessage);
            }

            _logger.LogInformation("Location details - City: {City}, Zip: {Zip}, Region: {Region}, Country: {Country}, Lat: {Lat}, Lon: {Lon}", location.City, location.Zip, location.RegionName, location.Country, location.Lat, location.Lon);
            
            var responseMessage = string.Format("Gotcha!! Toluwaloope location detector has detected your IP: {0} | Location: {1}, {2}, {3}, {4} (Lat: {5}, Long: {6})", ip, location.City, location.Zip, location.RegionName, location.Country, location.Lat, location.Lon);
            _logger.LogInformation("Successfully resolved location for IP {Ip}", ip);

            return new OkObjectResult(responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing caller information.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
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
