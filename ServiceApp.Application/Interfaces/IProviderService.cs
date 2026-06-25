using ServiceApp.Application.DTOs.Providers;

namespace ServiceApp.Application.Interfaces;

public interface IProviderService
{
    Task<ProviderDto> CreateProfileAsync(Guid userId, CreateProviderRequest request, CancellationToken cancellationToken = default);

    Task<ProviderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderDto>> SearchAsync(Guid? serviceId, CancellationToken cancellationToken = default);
}
