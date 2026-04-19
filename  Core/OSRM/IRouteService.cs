namespace Core.OSRM;

public interface IRouteService
{
    Task<RouteResult> GetRouteAsync(double fromLat, double fromLon, double toLat, double toLon);
    Task<RouteResult> GetRouteAsync(List<(double lat, double lon)> waypoints);
}