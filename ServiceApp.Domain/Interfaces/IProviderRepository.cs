using ServiceApp.Domain.Entities;

namespace ServiceApp.Domain.Interfaces;

public interface IProviderRepository : IRepository<ProviderProfile>
{
    /// <summary>Returns the provider profile (with User and Service loaded) for a given id.</summary>
    Task<ProviderProfile?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns the provider profile owned by the given user, if any.</summary>
    Task<ProviderProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Lists providers for a service category, optionally ordered by rating.</summary>
    Task<IReadOnlyList<ProviderProfile>> SearchAsync(
        Guid? serviceId,
        CancellationToken cancellationToken = default);
}
