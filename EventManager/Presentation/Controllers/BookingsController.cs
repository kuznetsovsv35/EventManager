using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

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
        return Accepted(new Uri($"/bookings/{booking.Id}", UriKind.Relative), booking);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BookingInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> GetBooking(Guid id, CancellationToken cancellation)
        => Ok(await bookingService.GetBookingByIdAsync(id, cancellation));
}