using Core.Geocoding;
using Core.Storage;

namespace Core.Customer;

public class Customer : IIdentifiable
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Email { get; set; }
    public required string PhoneNumber { get; set; }

    public string? CompanyName { get; set; }
    public string? Ico { get; set; }
    public string? Dic { get; set; }

    public GeoLocation? DefaultPickupLocation { get; set; }
    public GeoLocation? DefaultDeliveryLocation { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}