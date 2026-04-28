namespace Core.Trip;

public enum TripStatus
{
    Scheduled,
    OnGoing,
    Completed
}

public class Trip(
    DateOnly departureDate,
    List<Order.Order> orders,
    TripStatus status = TripStatus.Scheduled
)
{
    public DateOnly DepartureDate { get; set; } = departureDate;
    public TripStatus Status { get; set; } = status;
    public List<Order.Order> Orders { get; set; } = orders;
}