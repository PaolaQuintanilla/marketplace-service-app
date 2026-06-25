using ServiceApp.Domain.Entities;

namespace ServiceApp.Domain.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
