using Core.Order;
using Core.Storage;

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
) : IIdentifiable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateOnly DepartureDate { get; set; } = departureDate;
    public TripStatus Status { get; private set; } = status;
    public List<Order.Order> Orders { get; set; } = orders;

    public void Start()
    {
        if (Status != TripStatus.Scheduled)
        {
            throw new InvalidOperationException("Trip has already been started");
        }
        Status = TripStatus.OnGoing;
        foreach (var order in Orders)
        {
            order.Status = OrderStatus.EnRoute;
        }
    }

    public void Complete()
    {
        if (Status != TripStatus.OnGoing)
        {
            throw new InvalidOperationException("Cannot complete a trip that is not ongoing");
        }
        Status = TripStatus.Completed;
        foreach (var order in Orders)
        {
            order.Status = OrderStatus.Delivered;
        }
    }
}