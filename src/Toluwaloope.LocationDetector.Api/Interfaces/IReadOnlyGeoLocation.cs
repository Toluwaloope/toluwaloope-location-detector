namespace Toluwaloope.LocationDetector.Api.Interfaces;

/// <summary>
/// Read-only interface for geolocation data.
/// Provides access to geolocation information without allowing modifications.
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
}
