using ServiceApp.Application.DTOs.Services;

namespace ServiceApp.Application.Interfaces;

public interface IServiceCatalogService
{
    Task<IReadOnlyList<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ServiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
}
