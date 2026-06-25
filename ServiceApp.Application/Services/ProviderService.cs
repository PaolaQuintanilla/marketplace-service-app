using AutoMapper;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Providers;
using ServiceApp.Application.Interfaces;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Application.Services;

public class ProviderService(IUnitOfWork unitOfWork, IMapper mapper) : IProviderService
{
    public async Task<ProviderDto> CreateProfileAsync(Guid userId, CreateProviderRequest request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (await unitOfWork.Providers.GetByUserIdAsync(userId, cancellationToken) is not null)
            throw new ConflictException("This user already has a provider profile.");

        if (!await unitOfWork.Services.ExistsAsync(request.ServiceId, cancellationToken))
            throw new ValidationException($"Service '{request.ServiceId}' does not exist.");

        var profile = new ProviderProfile
        {
            UserId = userId,
            ServiceId = request.ServiceId,
            Description = request.Description.Trim(),
            HourlyRate = request.HourlyRate,
            Rating = 0
        };

        // Becoming a provider promotes the account's role.
        user.Role = UserRole.Provider;
        unitOfWork.Users.Update(user);

        await unitOfWork.Providers.AddAsync(profile, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(profile.Id, cancellationToken);
    }

    public async Task<ProviderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await unitOfWork.Providers.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Provider '{id}' was not found.");

        return mapper.Map<ProviderDto>(profile);
    }

    public async Task<IReadOnlyList<ProviderDto>> SearchAsync(Guid? serviceId, CancellationToken cancellationToken = default)
    {
        var providers = await unitOfWork.Providers.SearchAsync(serviceId, cancellationToken);
        return mapper.Map<IReadOnlyList<ProviderDto>>(providers);
    }
}
