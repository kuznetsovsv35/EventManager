using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IBookingQueue
{
    void EnqueueBooking(Booking booking);

    Task<Booking> DequeueBooking(CancellationToken cancellation);
}