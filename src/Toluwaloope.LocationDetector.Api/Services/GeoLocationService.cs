using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
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
    private readonly TelemetryClient _telemetryClient;
    private const string ipApiUrl = "http://ip-api.com/json/";

    public GeoLocationService(
        ILogger<GeoLocationService> logger,
        IHttpClientFactory httpClientFactory,
        TelemetryClient telemetryClient)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _telemetryClient = telemetryClient;
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
    public async Task<IGeoLocation?> GetGeoLocationAsync(string ip, string correlationId)
    {
        var dependencyStart = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var dependency = new DependencyTelemetry
        {
            Type = "HTTP",
            Target = "ip-api.com",
            Name = "GET /json/{ip}",
            Data = $"{ipApiUrl}{ip}",
            Timestamp = dependencyStart
        };
        dependency.Properties["CorrelationId"] = correlationId;
        dependency.Properties["ClientIp"] = ip;

        try
        {
            _logger.LogInformation("Geo lookup started for IP {Ip}", ip);

            using var response = await _httpClient.GetAsync($"{ipApiUrl}{ip}");
            var payload = await response.Content.ReadAsStringAsync();

            dependency.ResultCode = ((int)response.StatusCode).ToString();
            dependency.Success = response.IsSuccessStatusCode;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Geo lookup failed for IP {Ip}. StatusCode: {StatusCode}", ip, response.StatusCode);
                _telemetryClient.TrackEvent("GeoLookup.HttpFailure", new Dictionary<string, string>
                {
                    ["CorrelationId"] = correlationId,
                    ["ClientIp"] = ip,
                    ["StatusCode"] = dependency.ResultCode
                });
                return null;
            }

            _logger.LogInformation("Geo lookup response received for IP {Ip}", ip);

            var model = JsonSerializer.Deserialize<GeoLocationModel>(payload);
            
            if (model == null)
            {
                _logger.LogWarning("Deserialized null geo location model for IP {Ip}", ip);
                _telemetryClient.TrackEvent("GeoLookup.DeserializedNull", new Dictionary<string, string>
                {
                    ["CorrelationId"] = correlationId,
                    ["ClientIp"] = ip
                });
                return null;
            }

            _telemetryClient.TrackEvent("GeoLookup.Succeeded", new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["ClientIp"] = ip,
                ["City"] = model.City,
                ["Region"] = model.RegionName,
                ["Country"] = model.Country
            });

            // Return the deserialized model
            return model;
        }
        catch (JsonException ex)
        {
            dependency.Success = false;
            dependency.ResultCode = "JsonException";
            _logger.LogError(ex, "Failed to deserialize geo lookup response for IP {Ip}", ip);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["ClientIp"] = ip,
                ["FailureType"] = "JsonException"
            });
            return null;
        }
        catch (HttpRequestException ex)
        {
            dependency.Success = false;
            dependency.ResultCode = "HttpRequestException";
            _logger.LogError(ex, "Failed to retrieve geo lookup for IP {Ip}", ip);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                ["CorrelationId"] = correlationId,
                ["ClientIp"] = ip,
                ["FailureType"] = "HttpRequestException"
            });
            return null;
        }
        finally
        {
            stopwatch.Stop();
            dependency.Duration = stopwatch.Elapsed;
            _telemetryClient.TrackDependency(dependency);
        }
    }
}
