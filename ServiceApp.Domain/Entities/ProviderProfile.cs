using ServiceApp.Domain.Common;

namespace ServiceApp.Domain.Entities;

public class ProviderProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid ServiceId { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal HourlyRate { get; set; }

    public double Rating { get; set; }

    public User User { get; set; } = null!;

    public Service Service { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
