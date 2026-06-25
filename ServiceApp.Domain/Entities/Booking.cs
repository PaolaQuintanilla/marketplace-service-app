using ServiceApp.Domain.Common;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid ClientId { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ServiceId { get; set; }

    public DateTime ScheduledFor { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? Notes { get; set; }

    public User Client { get; set; } = null!;

    public ProviderProfile Provider { get; set; } = null!;

    public Service Service { get; set; } = null!;
}
