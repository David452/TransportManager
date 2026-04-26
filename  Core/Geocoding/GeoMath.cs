namespace Core.Geocoding;

/*
 * Matematika spojena s vzdialenostami medzi suradnicami
 * zdroj: https://movable-type.co.uk/scripts/latlong.html
 */
public static class GeoMath
{
    
    private const double EarthRadiusKm = 6361.0;
    
    public static double PointToSegmentDistanceKm(GeoLocation point, GeoLocation segA, GeoLocation segB)
    {
        if (segA.Latitude == segB.Latitude && segA.Longitude == segB.Longitude)
            return HaversineDistanceKm(point, segA);

        var crossTrack = CrossTrackDistanceKm(point, segA, segB);
        var alongTrack = AlongTrackDistanceKm(point, segA, segB);

        if (alongTrack < 0)
            return HaversineDistanceKm(point, segA);

        return alongTrack > HaversineDistanceKm(segA, segB) ? HaversineDistanceKm(point, segB) : Math.Abs(crossTrack);
    }

    public static double PointToRouteDistanceKm(GeoLocation point, IReadOnlyList<(double Lat, double Lon)> polyline)
    {
        if (polyline.Count < 2)
            return double.MaxValue;

        var min = double.MaxValue;
        for (var i = 0; i < polyline.Count - 1; i++)
        {
            var a = new GeoLocation()
            {
                Latitude = polyline[i].Lat,
                Longitude = polyline[i].Lon,
            };
            var b = new GeoLocation()
            {
                Latitude = polyline[i + 1].Lat,
                Longitude = polyline[i + 1].Lon,
            };
            var d = PointToSegmentDistanceKm(point, a, b);
            if (d < min) min = d;
        }

        return min;
    }

    /*
     * Kolma vzdialenost bodu od velkokruhovej linie
     */
    private static double CrossTrackDistanceKm(GeoLocation point, GeoLocation a, GeoLocation b)
    {
        var angularDistAC = HaversineDistanceKm(a, point) / EarthRadiusKm;
        var bearingAC = BearingRad(a, point);
        var bearingAB = BearingRad(a, b);
        
        return Math.Asin(Math.Sin(angularDistAC) * Math.Sin(bearingAC - bearingAB)) * EarthRadiusKm;
    }
    
    /*
     * vzdialenost pozdlz linie od bodu A k pate kolmice z point
     */
    private static double AlongTrackDistanceKm(GeoLocation point, GeoLocation a, GeoLocation b)
    {
        var angularDistAC = HaversineDistanceKm(a, point) / EarthRadiusKm;
        var crossTrackAngular = CrossTrackDistanceKm(point, a, b) / EarthRadiusKm;

        return Math.Acos(Math.Cos(angularDistAC) / Math.Cos(crossTrackAngular)) * EarthRadiusKm;
    }

    private static double BearingRad(GeoLocation from, GeoLocation to)
    {
        var lat1 = from.Latitude * Math.PI / 180;
        var lat2 = to.Latitude * Math.PI / 180;
        var deltaLon = (to.Longitude - from.Longitude) * Math.PI / 180;
        
        var y = Math.Sin(deltaLon) * Math.Cos(lat2);
        var x = Math.Cos(lat1) * Math.Sin(lat2) -
                Math.Sin(lat1) * Math.Sin(lat2) * Math.Cos(deltaLon);
        return Math.Atan2(y, x);
    }
    
    private static double HaversineDistanceKm(GeoLocation point1, GeoLocation point2)
    {
        var fi1 = point1.Latitude * Math.PI / 180;
        var fi2 = point2.Latitude * Math.PI / 180;
        var deltaLambda = (point1.Longitude - point2.Longitude) * Math.PI / 180.0;
        var deltaFi = (point1.Latitude - point2.Latitude)  * Math.PI / 180.0;

        var a = (Math.Sin(deltaFi / 2) * Math.Sin(deltaFi / 2)) + Math.Cos(fi1) * Math.Cos(fi2) * (Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2));
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var d = EarthRadiusKm * c;
        return d;
    }
}