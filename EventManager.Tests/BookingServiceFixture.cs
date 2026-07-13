using EventManager.Application.Interfaces;
using EventManager.Application.Services;

namespace EventManager.Tests;

public class BookingServiceFixture  : TestAppDbContext
{
    public IBookingService BookingService { get; }

    public BookingServiceFixture() : base()
        => BookingService = new BookingService(this, new BookingQueue());
}