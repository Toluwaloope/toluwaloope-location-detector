using System.Text.Json.Serialization;
using Toluwaloope.LocationDetector.Api.Interfaces;

namespace Toluwaloope.LocationDetector.Api.Models;

/// <summary>
/// Represents geolocation information for an IP address.
/// </summary>
public class GeoLocationModel : IGeoLocation
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("regionName")]
    public string RegionName { get; set; } = string.Empty;

    [JsonPropertyName("zip")]
    public string Zip { get; set; } = string.Empty;

    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("lat")]
    public float Lat { get; set; }

    [JsonPropertyName("lon")]
    public float Lon { get; set; }

    public Task<IGeoLocation?> GetGeoLocationAsync(string ip, string correlationId)
    {
        throw new NotImplementedException("Use GeoLocationService for retrieving geolocation data.");
    }
}
