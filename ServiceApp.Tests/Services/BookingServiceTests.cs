using Moq;
using ServiceApp.Application.Common.Exceptions;
using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Application.Services;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;
using ServiceApp.Domain.Interfaces;
using ServiceApp.Tests.TestSupport;

namespace ServiceApp.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IProviderRepository> _providers = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly BookingService _sut;

    public BookingServiceTests()
    {
        _uow.Setup(u => u.Providers).Returns(_providers.Object);
        _uow.Setup(u => u.Bookings).Returns(_bookings.Object);
        _sut = new BookingService(_uow.Object, TestMapper.Create());
    }

    /// <summary>A booking with all navigation properties populated so AutoMapper can map it.</summary>
    private static Booking PopulatedBooking(Guid clientId, Guid providerUserId, BookingStatus status = BookingStatus.Pending)
    {
        var service = new Service { Name = "Plumbing" };
        return new Booking
        {
            ClientId = clientId,
            ScheduledFor = new DateTime(2030, 1, 1, 9, 0, 0, DateTimeKind.Utc),
            Status = status,
            Client = new User { Name = "Client" },
            Service = service,
            Provider = new ProviderProfile
            {
                UserId = providerUserId,
                User = new User { Name = "Provider" },
                Service = service
            }
        };
    }

    // ---- CreateAsync ----

    [Fact]
    public async Task CreateAsync_WhenProviderNotFound_ThrowsNotFoundException()
    {
        _providers.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((ProviderProfile?)null);
        var request = new CreateBookingRequest(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task CreateAsync_WhenBookingOwnService_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var provider = new ProviderProfile { Id = Guid.NewGuid(), UserId = userId };
        _providers.Setup(r => r.GetWithDetailsAsync(provider.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(provider);
        var request = new CreateBookingRequest(provider.Id, DateTime.UtcNow.AddDays(1), null);

        // The client is the same user that owns the provider profile.
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(userId, request));
    }

    [Fact]
    public async Task CreateAsync_WhenScheduledInThePast_ThrowsValidationException()
    {
        var provider = new ProviderProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid() };
        _providers.Setup(r => r.GetWithDetailsAsync(provider.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(provider);
        var request = new CreateBookingRequest(provider.Id, DateTime.UtcNow.AddDays(-1), null);

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(Guid.NewGuid(), request));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsPendingBookingAndReturnsDto()
    {
        var clientId = Guid.NewGuid();
        var provider = new ProviderProfile { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ServiceId = Guid.NewGuid() };
        _providers.Setup(r => r.GetWithDetailsAsync(provider.Id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(provider);

        Booking? added = null;
        _bookings.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>()))
                 .Callback<Booking, CancellationToken>((b, _) => added = b)
                 .Returns(Task.CompletedTask);
        // After persisting, the service re-reads the booking with details for the response.
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(PopulatedBooking(clientId, provider.UserId));

        var request = new CreateBookingRequest(provider.Id, DateTime.UtcNow.AddDays(3), "  please ring the bell  ");

        var dto = await _sut.CreateAsync(clientId, request);

        Assert.NotNull(added);
        Assert.Equal(clientId, added!.ClientId);
        Assert.Equal(provider.Id, added.ProviderId);
        Assert.Equal(provider.ServiceId, added.ServiceId);
        Assert.Equal(BookingStatus.Pending, added.Status);
        Assert.Equal("please ring the bell", added.Notes);       // trimmed
        Assert.Equal(DateTimeKind.Utc, added.ScheduledFor.Kind); // coerced to UTC
        Assert.Equal("Pending", dto.Status);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- UpdateStatusAsync ----

    [Fact]
    public async Task UpdateStatusAsync_WhenBookingNotFound_ThrowsNotFoundException()
    {
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((Booking?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateStatusAsync(Guid.NewGuid(), Guid.NewGuid(), BookingStatus.Confirmed));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenCallerIsNotParticipant_ThrowsForbiddenException()
    {
        var booking = PopulatedBooking(Guid.NewGuid(), Guid.NewGuid());
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(booking);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.UpdateStatusAsync(booking.Id, Guid.NewGuid(), BookingStatus.Cancelled));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenClientTriesToConfirm_ThrowsForbiddenException()
    {
        var clientId = Guid.NewGuid();
        var booking = PopulatedBooking(clientId, Guid.NewGuid());
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(booking);

        // Only the provider may confirm/complete.
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.UpdateStatusAsync(booking.Id, clientId, BookingStatus.Confirmed));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenMovingBackToPending_ThrowsValidationException()
    {
        var providerUserId = Guid.NewGuid();
        var booking = PopulatedBooking(Guid.NewGuid(), providerUserId);
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(booking);

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.UpdateStatusAsync(booking.Id, providerUserId, BookingStatus.Pending));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenBookingAlreadyCompleted_ThrowsValidationException()
    {
        var providerUserId = Guid.NewGuid();
        var booking = PopulatedBooking(Guid.NewGuid(), providerUserId, BookingStatus.Completed);
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(booking);

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.UpdateStatusAsync(booking.Id, providerUserId, BookingStatus.Cancelled));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenProviderConfirmsPendingBooking_UpdatesAndReturnsDto()
    {
        var providerUserId = Guid.NewGuid();
        var booking = PopulatedBooking(Guid.NewGuid(), providerUserId);
        _bookings.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(booking);

        var dto = await _sut.UpdateStatusAsync(booking.Id, providerUserId, BookingStatus.Confirmed);

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.UpdatedAt);
        Assert.Equal("Confirmed", dto.Status);
        _bookings.Verify(r => r.Update(booking), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
