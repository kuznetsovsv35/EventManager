using BookingService.Application;
using BookingService.Application.DTO;
using BookingService.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers;

/// <summary>
/// Контролер управления бронированием.
/// </summary>
/// <param name="bookingService"></param>
[ApiController]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost("/events/{eventId:guid}/book")]
    [ProducesResponseType<BookingInfo>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Policy = Policy.CreateBooking)]
    public async Task<ActionResult<BookingInfo>> CreateBookingAsync([FromRoute] Guid eventId, CancellationToken cancellation)
    {
        BookingInfo booking = await bookingService.CreateBookingAsync(eventId, cancellation);
        return Accepted(new Uri($"/bookings/{booking.Id}", UriKind.Relative), booking);
    }

    [HttpGet("/bookings/{id:guid}")]
    [ProducesResponseType<BookingInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Policy = Policy.GetBooking)]
    public async Task<ActionResult<BookingInfo>> GetBooking(Guid id, CancellationToken cancellation)
        => Ok(await bookingService.GetBookingByIdAsync(id, cancellation));

    [HttpDelete("/bookings/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = Policy.CancelBooking)]
    public async Task<ActionResult> CancelBooking(Guid id, CancellationToken cancellation)
    {
        await bookingService.CancelBookingAsync(id, cancellation);
        return NoContent();
    }
}