namespace ServiceApp.Domain.Interfaces;

/// <summary>
/// Coordinates the repositories and commits all changes in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }

    IServiceRepository Services { get; }

    IProviderRepository Providers { get; }

    IBookingRepository Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
