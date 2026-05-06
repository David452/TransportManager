namespace Core.OSRM;

public class RouteResult
{
    public double DistanceMeters { get; set; }
    public double DurationSeconds { get; set; }
    public string EncodedPolyline { get; set; } = string.Empty;
    
    public double DistanceKm => DistanceMeters / 1000;
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
}