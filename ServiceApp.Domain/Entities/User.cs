using ServiceApp.Domain.Common;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;

    // One-to-one: a user with the Provider role has a profile.
    public ProviderProfile? ProviderProfile { get; set; }

    // Bookings this user placed as a client.
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
