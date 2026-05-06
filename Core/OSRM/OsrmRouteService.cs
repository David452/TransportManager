using System.Text.Json;
using Core.Geocoding;

namespace Core.OSRM;

/**
 * OSRM je externé API, ktoré vracia zakódovanú polyline medzi 2 lokáciami
 */
public class OsrmRouteService(HttpClient httpClient) : IRouteService
{
    private const string BaseUrl = "https://router.project-osrm.org";

    public async Task<RouteResult> GetRouteAsync(double fromLat, double fromLon, double toLat, double toLon)
    {
        var waypoints = new List<(double lat, double lon)>
        {
            (fromLat, fromLon),
            (toLat, toLon)
        };

        return await GetRouteAsync(waypoints);
    }

    public async Task<RouteResult> GetRouteAsync(GeoLocation from, GeoLocation to)
    {
        var waypoints = new List<GeoLocation> { from, to };
        return await GetRouteAsync(waypoints);
    }

    public async Task<RouteResult> GetRouteAsync(List<GeoLocation> waypoints)
    {
        var mappedWaypoints = waypoints.Select(l => (l.Latitude, l.Longitude)).ToList();
        return await GetRouteAsync(mappedWaypoints);
    }

    public async Task<RouteResult> GetRouteAsync(List<(double lat, double lon)> waypoints)
    {
        var coords = string.Join(";", waypoints.Select(w => FormattableString.Invariant($"{w.lon},{w.lat}")));

        var url = $"{BaseUrl}/route/v1/driving/{coords}?overview=full&geometries=polyline";
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var route = doc.RootElement.GetProperty("routes")[0];

        return new RouteResult
        {
            DistanceMeters = route.GetProperty("distance").GetDouble(),
            DurationSeconds = route.GetProperty("duration").GetDouble(),
            EncodedPolyline = route.GetProperty("geometry").GetString() ?? string.Empty,
        };
    }
}