using System.ComponentModel.DataAnnotations;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Application.DTOs.Bookings;

public record BookingDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid ProviderId,
    string ProviderName,
    Guid ServiceId,
    string ServiceName,
    DateTime ScheduledFor,
    string Status,
    string? Notes,
    DateTime CreatedAt);

public record CreateBookingRequest(
    [Required] Guid ProviderId,
    [Required] DateTime ScheduledFor,
    [MaxLength(500)] string? Notes);

public record UpdateBookingStatusRequest([Required] BookingStatus Status);
