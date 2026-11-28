using BookingService.Application;
using BookingService.Application.DTO;
using BookingService.Database.Repositories;
using BookingService.Domain;
using Common.Tests;

namespace BookingService.IntegrationTests;

[Collection(nameof(ContainersCollection))]
public class BookingRepositoryTest(ContainerWrapper testContainer) : DatabaseTestBase(testContainer)
{
    [Trait(Category.Name, Category.Repositories)]
    [Fact]
    public async Task AddBooking_Success()
    {
        // Given
        await ResetDatabase();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking(@eventId, userId);
        var infoBooking = booking.ToInfo();

        // When
        await using var workContext = CreateDbContext();
        IBookingRepository workRepository = new BookingRepository(workContext);
        await workRepository.AddBookingAsync(booking, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IBookingRepository verifyRepository = new BookingRepository(verifyContext);
        var bookingFound = await verifyRepository.GetBookingAsync(booking.Id, CancellationToken.None);

        Assert.Equal(eventId, bookingFound?.EventId);
        Assert.Equal(infoBooking, bookingFound?.ToInfo());
    }

    [Trait(Category.Name, Category.Repositories)]
    [Fact]
    public async Task UpdateBooking_Success()
    {
        // Given
        await ResetDatabase();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var booking = new Booking(eventId, userId);

        await using var context = CreateDbContext();
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

        Assert.Equal(eventId, updated?.EventId);
        Assert.Equal(updating.ToInfo(), updated?.ToInfo());
    }

    [Trait(Category.Name, Category.Repositories)]
    [Fact]
    public async Task GetPendingBookings_Success()
    {
        // Given
        await ResetDatabase();

        var userId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var bookingPending = new Booking(eventId, userId);
        var bookingConfirmed = new Booking(eventId, userId);
        var infoPending = bookingPending.ToInfo();

        // When
        await using var workContext = CreateDbContext();
        IBookingRepository workRepository = new BookingRepository(workContext);
        await workRepository.AddBookingAsync(bookingPending, CancellationToken.None);
        await workRepository.AddBookingAsync(bookingConfirmed, CancellationToken.None);
        bookingConfirmed.Confirm();
        await workRepository.UpdateBookingStatusAsync(bookingConfirmed, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IBookingRepository verifyRepository = new BookingRepository(verifyContext);
        var bookingFound = (await verifyRepository.GetPendingBookingsAsync(1, CancellationToken.None).SingleAsync()).SingleOrDefault();

        Assert.NotNull(bookingFound);
        Assert.Equal(infoPending, bookingFound?.ToInfo());
    }
}