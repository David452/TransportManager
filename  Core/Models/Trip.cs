namespace Core.Models;

public enum TripStatus
{
    Scheduled,
    OnGoing,
    Completed
}

public class Trip(
    DateTime departureDate,
    List<Order> orders,
    TripStatus status = TripStatus.Scheduled
)
{
    public DateTime DepartureDate { get; set; } = departureDate;
    public TripStatus Status { get; set; } = status;
    public List<Order> Orders { get; set; } = orders;
}