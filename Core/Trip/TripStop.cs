namespace Core.Trip;

public enum StopType
{
    Pickup,
    Dropoff
}

public record TripStop(Guid OrderId, StopType Type);