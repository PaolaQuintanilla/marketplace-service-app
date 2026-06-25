using AutoMapper;
using ServiceApp.Application.DTOs.Auth;
using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Application.DTOs.Providers;
using ServiceApp.Application.DTOs.Services;
using ServiceApp.Domain.Entities;

namespace ServiceApp.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForCtorParam(nameof(UserDto.Role), o => o.MapFrom(u => u.Role.ToString()));

        CreateMap<Service, ServiceDto>();

        CreateMap<ProviderProfile, ProviderDto>()
            .ForCtorParam(nameof(ProviderDto.UserName), o => o.MapFrom(p => p.User.Name))
            .ForCtorParam(nameof(ProviderDto.ServiceName), o => o.MapFrom(p => p.Service.Name));

        CreateMap<Booking, BookingDto>()
            .ForCtorParam(nameof(BookingDto.ClientName), o => o.MapFrom(b => b.Client.Name))
            .ForCtorParam(nameof(BookingDto.ProviderName), o => o.MapFrom(b => b.Provider.User.Name))
            .ForCtorParam(nameof(BookingDto.ServiceName), o => o.MapFrom(b => b.Service.Name))
            .ForCtorParam(nameof(BookingDto.Status), o => o.MapFrom(b => b.Status.ToString()));
    }
}
