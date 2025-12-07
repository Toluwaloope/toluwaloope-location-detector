using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using Toluwaloope.LocationDetector.Api.Interfaces;

namespace Toluwaloope.LocationDetector.Api;

/// <summary>
/// Represents geolocation information for an IP address.
/// </summary>
public class GeoLocation : IGeoLocation
{
    private readonly ILogger<GeoLocation> _logger;

    public GeoLocation(ILogger<GeoLocation> logger)
    {
        _logger = logger;
    }

    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public float Lat { get; set; }
    public float Lon { get; set; }

    /// <summary>
    /// Retrieves geolocation information for the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to query for geolocation information.</param>
    /// <returns>A GeoLocation object containing the geolocation data, or null if deserialization fails.</returns>
    public async Task<IGeoLocation?> GetGeoLocationAsync(string ip)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
        _logger.LogInformation("Geo lookup response for IP {Ip}: {Payload}", ip, response);

        try
        {
            return JsonSerializer.Deserialize<GeoLocation>(response);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize geo lookup response for IP {Ip}. Payload: {Payload}", ip, response);
            return null;
        }
    }
}


