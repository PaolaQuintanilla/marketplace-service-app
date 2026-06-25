using ServiceApp.Domain.Entities;

namespace ServiceApp.Domain.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetForClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Booking>> GetForProviderAsync(Guid providerId, CancellationToken cancellationToken = default);
}
