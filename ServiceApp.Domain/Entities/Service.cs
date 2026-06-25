using ServiceApp.Domain.Common;

namespace ServiceApp.Domain.Entities;

public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<ProviderProfile> Providers { get; set; } = new List<ProviderProfile>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
