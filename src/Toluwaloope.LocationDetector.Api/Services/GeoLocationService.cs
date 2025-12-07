using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;
using Toluwaloope.LocationDetector.Api.Interfaces;
using Toluwaloope.LocationDetector.Api.Models;

namespace Toluwaloope.LocationDetector.Api.Services;

/// <summary>
/// Service for retrieving geolocation information from IP addresses.
/// </summary>
public class GeoLocationService : IGeoLocation
{
    private readonly ILogger<GeoLocationService> _logger;
    private readonly HttpClient _httpClient;
    private const string ipApiUrl = "http://ip-api.com/json/";

    public GeoLocationService(ILogger<GeoLocationService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
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
        try
        {
            var response = await _httpClient.GetStringAsync($"{ipApiUrl}{ip}");
            _logger.LogInformation("Geo lookup response for IP {Ip}: {Payload}", ip, response);

            var model = JsonSerializer.Deserialize<GeoLocationModel>(response);
            
            if (model == null)
            {
                _logger.LogWarning("Deserialized null geo location model for IP {Ip}", ip);
                return null;
            }

            // Map model to service properties
            return new GeoLocationService(_logger, null!)
            {
                Country = model.Country,
                City = model.City,
                RegionName = model.RegionName,
                Zip = model.Zip,
                Query = model.Query,
                Lat = model.Lat,
                Lon = model.Lon
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize geo lookup response for IP {Ip}", ip);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to retrieve geo lookup for IP {Ip}", ip);
            return null;
        }
    }
}
