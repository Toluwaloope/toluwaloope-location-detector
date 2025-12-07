using System.Text.Json.Serialization;

namespace Toluwaloope.LocationDetector.Api.Models;

/// <summary>
/// Represents geolocation information for an IP address.
/// </summary>
public class GeoLocationModel
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
}
