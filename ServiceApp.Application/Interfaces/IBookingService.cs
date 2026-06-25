using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(Guid clientId, CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<BookingDto> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingDto>> GetMyClientBookingsAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingDto>> GetMyProviderBookingsAsync(Guid currentUserId, CancellationToken cancellationToken = default);

    Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid currentUserId, BookingStatus status, CancellationToken cancellationToken = default);
}
