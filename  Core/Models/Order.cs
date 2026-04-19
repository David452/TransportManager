namespace Core.Models;

public enum OrderStatus
{
    New,
    Assigned, // zaradena do tripu
    EnRoute, // prave sa realizuje
    Delivered, // dorucena
    Cancelled, // zrusena
}

public class Order(string origin, string destination, OrderStatus status = OrderStatus.New, string? note = null)
{
    public readonly Guid Id = Guid.NewGuid();
    public string Origin { get; set; } = origin;
    public string Destination { get; set; } = destination;
    public OrderStatus Status { get; } = status;
    public string? Note { get; set; } = note;
    
}