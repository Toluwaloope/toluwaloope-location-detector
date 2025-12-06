using System.Net.Http;
using System.Text.Json;

namespace Toluwaloope.LocationDetector.Api;

public class GeoLocation
{
    public string Country { get; set; }
    public string City { get; set; }
    public string RegionName { get; set; }
    public string Query { get; set; }
    public float Lat { get; set; }
    public float Lon { get; set; }


    public async Task<GeoLocation> GetGeoLocationAsync(string ip)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ip}");
        return JsonSerializer.Deserialize<GeoLocation>(response);
    }
}


