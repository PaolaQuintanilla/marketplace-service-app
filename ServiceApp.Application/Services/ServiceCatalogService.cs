using AutoMapper;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Services;
using ServiceApp.Application.Interfaces;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Application.Services;

public class ServiceCatalogService(IUnitOfWork unitOfWork, IMapper mapper) : IServiceCatalogService
{
    public async Task<IReadOnlyList<ServiceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.Services.ListAllAsync(cancellationToken);
        return mapper.Map<IReadOnlyList<ServiceDto>>(services);
    }

    public async Task<ServiceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.Services.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Service '{id}' was not found.");

        return mapper.Map<ServiceDto>(service);
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = new Service
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Description = request.Description?.Trim()
        };

        await unitOfWork.Services.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ServiceDto>(service);
    }
}
