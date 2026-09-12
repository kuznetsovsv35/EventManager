using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.IntegrationTests;

public class BookingRepositoryTest : DatabaseTestBase<AppDbContext>
{
    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task AddBooking_Success()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id);
        var infoBooking = booking.ToInfo();

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();

        // When
        await using var workContext = CreateDbContext();
        IBookingRepository workRepository = new BookingRepository(workContext);
        await workRepository.AddBookingAsync(booking, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IBookingRepository verifyRepository = new BookingRepository(verifyContext);
        var bookingFound = await verifyRepository.GetBookingAsync(booking.Id, CancellationToken.None);

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent, bookingFound.Event.ToOutputData());
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }
    
    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task UpdateBooking_Success()
    {
        // Given
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id);

        await using var context = CreateDbContext();
        context.Events.Add(@event);
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        // When
        await using var workContext = CreateDbContext();
        IBookingRepository workRepository = new BookingRepository(workContext);
        var updating = (await workRepository.GetBookingAsync(booking.Id, CancellationToken.None))!;
        updating.Confirm();
        await workRepository.UpdateBookingStatusAsync(updating, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IBookingRepository verifyRepository = new BookingRepository(verifyContext);
        var updated = await verifyRepository.GetBookingAsync(booking.Id, CancellationToken.None);

        Assert.NotNull(updated?.Event);
        Assert.Equal(updating.ToInfo(), updated.ToInfo());
        Assert.Equal(infoEvent, updated.Event.ToOutputData());
    }
}