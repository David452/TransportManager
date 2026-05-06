namespace Core.Geocoding;

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Latitude},{Longitude}:  {DisplayName}";
    }
}