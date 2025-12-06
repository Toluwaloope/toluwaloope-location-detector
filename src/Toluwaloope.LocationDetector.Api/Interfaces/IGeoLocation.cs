namespace Toluwaloope.LocationDetector.Api.Interfaces;

/// <summary>
/// Interface for geolocation services.
/// Provides methods to retrieve and access geolocation information.
/// </summary>
public interface IGeoLocation
{
    /// <summary>
    /// Gets the country name.
    /// </summary>
    string Country { get; }

    /// <summary>
    /// Gets the city name.
    /// </summary>
    string City { get; }

    /// <summary>
    /// Gets the region/state name.
    /// </summary>
    string RegionName { get; }

    /// <summary>
    /// Gets the IP address that was queried.
    /// </summary>
    string Query { get; }

    /// <summary>
    /// Gets the latitude coordinate.
    /// </summary>
    float Lat { get; }

    /// <summary>
    /// Gets the longitude coordinate.
    /// </summary>
    float Lon { get; }

    /// <summary>
    /// Retrieves geolocation information for the specified IP address asynchronously.
    /// </summary>
    /// <param name="ip">The IP address to query for geolocation information.</param>
    /// <returns>A task representing the asynchronous operation. The result is a GeoLocation object containing the geolocation data, or null if the operation fails.</returns>
    Task<IGeoLocation?> GetGeoLocationAsync(string ip);
}
