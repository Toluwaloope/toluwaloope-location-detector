using System.Net.Http;
using System.Text.Json;
using Toluwaloope.LocationDetector.Api.Interfaces;

namespace Toluwaloope.LocationDetector.Api;

/// <summary>
/// Represents geolocation information for an IP address.
/// </summary>
public class GeoLocation : IGeoLocation
{
    public string Country { get; set; }
    public string City { get; set; }
    public string RegionName { get; set; }
    public string Query { get; set; }
    public float Lat { get; set; }
    public float Lon { get; set; }

    /// <summary>
    /// Retrieves geolocation information for the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to query for geolocation information.</param>
    /// <returns>A GeoLocation object containing the geolocation data.</returns>
    public async Task<GeoLocation> GetGeoLocationAsync(string ip)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
        return JsonSerializer.Deserialize<GeoLocation>(response);
    }
}


