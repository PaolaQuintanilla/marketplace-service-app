using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceApp.API.Extensions;
using ServiceApp.Application.DTOs.Bookings;
using ServiceApp.Application.Interfaces;

namespace ServiceApp.API.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController(IBookingService bookings) : ControllerBase
{
    /// <summary>Create a booking as a client.</summary>
    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        var created = await bookings.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await bookings.GetByIdAsync(id, User.GetUserId(), ct));

    /// <summary>Bookings the current user placed as a client.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> Mine(CancellationToken ct)
        => Ok(await bookings.GetMyClientBookingsAsync(User.GetUserId(), ct));

    /// <summary>Bookings assigned to the current user as a provider.</summary>
    [HttpGet("assigned")]
    public async Task<ActionResult<IReadOnlyList<BookingDto>>> Assigned(CancellationToken ct)
        => Ok(await bookings.GetMyProviderBookingsAsync(User.GetUserId(), ct));

    /// <summary>Update a booking's status (confirm/complete by provider; cancel by either party).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<BookingDto>> UpdateStatus(Guid id, UpdateBookingStatusRequest request, CancellationToken ct)
        => Ok(await bookings.UpdateStatusAsync(id, User.GetUserId(), request.Status, ct));
}
