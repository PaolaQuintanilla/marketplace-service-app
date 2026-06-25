using Microsoft.EntityFrameworkCore;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Infrastructure.Persistence.Repositories;

public class BookingRepository(AppDbContext context) : Repository<Booking>(context), IBookingRepository
{
    private IQueryable<Booking> WithDetails() =>
        Set.Include(b => b.Client)
           .Include(b => b.Provider).ThenInclude(p => p.User)
           .Include(b => b.Service);

    public Task<Booking?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetForClientAsync(Guid clientId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .AsNoTracking()
            .Where(b => b.ClientId == clientId)
            .OrderByDescending(b => b.ScheduledFor)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Booking>> GetForProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
        => await WithDetails()
            .AsNoTracking()
            .Where(b => b.ProviderId == providerId)
            .OrderByDescending(b => b.ScheduledFor)
            .ToListAsync(cancellationToken);
}
