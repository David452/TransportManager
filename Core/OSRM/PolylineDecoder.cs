namespace Core.OSRM;

// Dekóduje encoded polyline (string) do zoznamu (lat, lon) bodov.
// Referencia algoritmu: https://developers.google.com/maps/documentation/utilities/polylinealgorithm
// Využitie generatívnej AI: implementácia dekódovacej slučky (bitový posun, zig-zag dekódovanie).
public static class PolylineDecoder
{
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