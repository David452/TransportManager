namespace Core.Geocoding;

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string FullAddress { get; set; } = string.Empty;

    public string DisplayName => FullAddress.Split(',').FirstOrDefault(p => !int.TryParse(p.Trim(), out _)) ?? FullAddress;
    public override string ToString()
    {
        return $"{Latitude},{Longitude}:  {FullAddress}";
    }
}