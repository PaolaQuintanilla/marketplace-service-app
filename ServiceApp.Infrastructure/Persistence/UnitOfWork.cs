using ServiceApp.Domain.Interfaces;
using ServiceApp.Infrastructure.Persistence.Repositories;

namespace ServiceApp.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(context);

    public IServiceRepository Services { get; } = new ServiceRepository(context);

    public IProviderRepository Providers { get; } = new ProviderRepository(context);

    public IBookingRepository Bookings { get; } = new BookingRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
