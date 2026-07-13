using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class BookingServiceFixture  : TestAppDbContext
{
    public IBookingService BookingService { get; }

    public BookingServiceFixture() : base()
        => BookingService = new BookingService(this, new AsyncQueue<Booking>());
}