using Microsoft.Extensions.DependencyInjection;
using ServiceApp.Application.Interfaces;
using ServiceApp.Application.Mapping;
using ServiceApp.Application.Services;

namespace ServiceApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
