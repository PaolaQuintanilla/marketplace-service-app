using Microsoft.EntityFrameworkCore;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Infrastructure.Persistence.Repositories;

public class ServiceRepository(AppDbContext context) : Repository<Service>(context), IServiceRepository
{
    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Set.AnyAsync(s => s.Id == id, cancellationToken);
}
