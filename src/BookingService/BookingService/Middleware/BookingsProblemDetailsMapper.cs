using BookingService.Domain.Exceptions;
using Common.Middleware.ErrorHandling;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Middleware;

public class BookingsProblemDetailsMapper : IProblemDetailsMapper
{
    public ProblemDetails? MapException(Exception exception)
        => exception switch
        {
            ActiveUserBookingException activeUserBooking => ProblemDetailsFactory.Conflict(activeUserBooking.Message),

            _ => null
        
        };
}
