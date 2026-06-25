using Microsoft.EntityFrameworkCore;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Infrastructure.Persistence.Repositories;

public class ProviderRepository(AppDbContext context) : Repository<ProviderProfile>(context), IProviderRepository
{
    public Task<ProviderProfile?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set.Include(p => p.User)
              .Include(p => p.Service)
              .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ProviderProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => Set.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<ProviderProfile>> SearchAsync(Guid? serviceId, CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Service)
            .AsQueryable();

        if (serviceId is not null)
            query = query.Where(p => p.ServiceId == serviceId);

        return await query
            .OrderByDescending(p => p.Rating)
            .ToListAsync(cancellationToken);
    }
}
