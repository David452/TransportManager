using Core.Geocoding;

namespace Core.Order;

public enum OrderStatus
{
    New,
    Assigned, // zaradena do tripu
    EnRoute, // prave sa realizuje
    Delivered, // dorucena
    Cancelled, // zrusena
}

public class Order(GeoLocation origin, GeoLocation destination, string? note = null)
{
    public readonly Guid Id = Guid.NewGuid();
    public GeoLocation Origin { get; set; } = origin;
    public GeoLocation Destination { get; set; } = destination;
    
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public string? Note { get; set; } = note;
    
}