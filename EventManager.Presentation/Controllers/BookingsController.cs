using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// Контролер управления бронированием.
/// </summary>
/// <param name="bookingService"></param>
[ApiController]
[Route("[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpPost("/Events/{eventId:guid}/book")]
    [ProducesResponseType<BookingInfo>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingInfo>> CreateBookingAsync([FromRoute] Guid eventId, CancellationToken cancellation)
    {
        BookingInfo booking = await bookingService.CreateBookingAsync(eventId, cancellation);
        return Accepted(new Uri($"/Bookings/{booking.Id}", UriKind.Relative), booking);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BookingInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> GetBooking(Guid id, CancellationToken cancellation)
        => Ok(await bookingService.GetBookingByIdAsync(id, cancellation));
}