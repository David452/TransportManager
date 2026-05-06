namespace Core.OSRM;

public static class PolylineDecoder
{
    /**
     * Referencia algoritmu: https://developers.google.com/maps/documentation/utilities/polylinealgorithm
     * Slúži na dekódovanie polyline (string) do listu
     */
    public static IReadOnlyList<(double Lat, double Lon)> Decode(string polyline)
    {
        var points = new List<(double Latitude, double Longitude)>();
        var index = 0;
        var lat = 0;
        var lon = 0;

        while (index < polyline.Length)
        {
            lat += DecodeNextValue(polyline, ref index);
            lon += DecodeNextValue(polyline, ref index);
            
            points.Add((lat / 1e5, lon / 1e5));
        }

        return points;
    }

    private static int DecodeNextValue(string encoded, ref int index)
    {
        int result = 0;
        int shift = 0;
        int b;

        do
        {
            b = encoded[index++] - 63;
            result |= (b & 0b00011111) << shift;
            shift += 5;
        } while (b >= 0x20);

        return (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
    }
}