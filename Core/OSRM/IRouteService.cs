using Core.Geocoding;

namespace Core.OSRM;

public interface IRouteService
{
    Task<RouteResult> GetRouteAsync(double fromLat, double fromLon, double toLat, double toLon);
    Task<RouteResult> GetRouteAsync(List<(double lat, double lon)> waypoints);
    Task<RouteResult> GetRouteAsync(GeoLocation from, GeoLocation to);
    Task<RouteResult> GetRouteAsync(List<GeoLocation> waypoints);
}