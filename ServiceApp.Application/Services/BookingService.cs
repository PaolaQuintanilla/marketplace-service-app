using AutoMapper;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Application.Interfaces;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;
using ServiceApp.Domain.Interfaces;

namespace ServiceApp.Application.Services;

public class BookingService(IUnitOfWork unitOfWork, IMapper mapper) : IBookingService
{
    public async Task<BookingDto> CreateAsync(Guid clientId, CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await unitOfWork.Providers.GetWithDetailsAsync(request.ProviderId, cancellationToken)
            ?? throw new NotFoundException($"Provider '{request.ProviderId}' was not found.");

        if (provider.UserId == clientId)
            throw new ValidationException("You cannot book your own services.");

        var scheduledFor = DateTime.SpecifyKind(request.ScheduledFor, DateTimeKind.Utc);
        if (scheduledFor <= DateTime.UtcNow)
            throw new ValidationException("The booking time must be in the future.");

        var booking = new Booking
        {
            ClientId = clientId,
            ProviderId = provider.Id,
            ServiceId = provider.ServiceId,
            ScheduledFor = scheduledFor,
            Status = BookingStatus.Pending,
            Notes = request.Notes?.Trim()
        };

        await unitOfWork.Bookings.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetAuthorizedAsync(booking.Id, clientId, cancellationToken);
    }

    public Task<BookingDto> GetByIdAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
        => GetAuthorizedAsync(id, currentUserId, cancellationToken);

    public async Task<IReadOnlyList<BookingDto>> GetMyClientBookingsAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var bookings = await unitOfWork.Bookings.GetForClientAsync(clientId, cancellationToken);
        return mapper.Map<IReadOnlyList<BookingDto>>(bookings);
    }

    public async Task<IReadOnlyList<BookingDto>> GetMyProviderBookingsAsync(Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var profile = await unitOfWork.Providers.GetByUserIdAsync(currentUserId, cancellationToken)
            ?? throw new ForbiddenException("You do not have a provider profile.");

        var bookings = await unitOfWork.Bookings.GetForProviderAsync(profile.Id, cancellationToken);
        return mapper.Map<IReadOnlyList<BookingDto>>(bookings);
    }

    public async Task<BookingDto> UpdateStatusAsync(Guid bookingId, Guid currentUserId, BookingStatus status, CancellationToken cancellationToken = default)
    {
        var booking = await unitOfWork.Bookings.GetWithDetailsAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException($"Booking '{bookingId}' was not found.");

        var isClient = booking.ClientId == currentUserId;
        var isProvider = booking.Provider.UserId == currentUserId;

        if (!isClient && !isProvider)
            throw new ForbiddenException("You are not a participant in this booking.");

        switch (status)
        {
            case BookingStatus.Confirmed or BookingStatus.Completed when !isProvider:
                throw new ForbiddenException("Only the provider can confirm or complete a booking.");
            case BookingStatus.Pending:
                throw new ValidationException("A booking cannot be moved back to pending.");
        }

        if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled)
            throw new ValidationException($"A {booking.Status.ToString().ToLowerInvariant()} booking can no longer be changed.");

        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Bookings.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<BookingDto>(booking);
    }

    private async Task<BookingDto> GetAuthorizedAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork.Bookings.GetWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Booking '{id}' was not found.");

        if (booking.ClientId != currentUserId && booking.Provider.UserId != currentUserId)
            throw new ForbiddenException("You are not a participant in this booking.");

        return mapper.Map<BookingDto>(booking);
    }
}
