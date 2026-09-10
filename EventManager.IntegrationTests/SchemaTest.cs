using EventManager.Application.DataTransfer;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManager.IntegrationTests;

public class SchemaTest : DatabaseTestBase
{
    [Trait(Category, Category_Database)]
    [Fact]
    public async Task DatabaseConnect_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();
    
        // When
        await using var context = CreateDbContext<AppDbContext>();        
    
        // Then
        Assert.True(context.Database.CanConnect());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task InsertEvent_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();

        Event eventToAdd = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoToAdd = eventToAdd.ToOutputData();

        // When
        await using var work = CreateDbContext<AppDbContext>();
        work.Events.Add(eventToAdd);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext<AppDbContext>();
        var eventAdded = await verify.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == eventToAdd.Id);
        var infoAdded = eventAdded?.ToOutputData();

        Assert.NotNull(infoAdded);
        Assert.Equal(infoToAdd, infoAdded);
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task UpdateEvent_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();

        Event origin = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        
        await using var create = CreateDbContext<AppDbContext>();
        create.Events.Add(origin);
        await create.SaveChangesAsync();

        // When
        await using var update = CreateDbContext<AppDbContext>();
        var modified = await update.Events.AsNoTracking().SingleAsync(e => e.Id == origin.Id);
        modified.Title = "New Title";
        modified.Description = "New Description";
        modified.StartAt = origin.StartAt.AddHours(2);
        modified.EndAt = DateTime.UtcNow.AddHours(2);
        update.Events.Update(modified);
        var updateCount = await update.SaveChangesAsync();
        var infoModified = modified.ToOutputData();
    
        // Then
        await using var verify = CreateDbContext<AppDbContext>();
        var updated = await verify.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == origin.Id);
        var infoUpdated = updated?.ToOutputData();

        Assert.Equal(1, updateCount);
        Assert.NotNull(infoUpdated);
        Assert.Equal(infoModified, infoUpdated);
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task DeleteEvent_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();

        Event origin = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        
        await using var create = CreateDbContext<AppDbContext>();
        create.Events.Add(origin);
        await create.SaveChangesAsync();
        var infoOrigin = origin.ToOutputData();

        // When
        await using var delete = CreateDbContext<AppDbContext>();
        var deleting = await delete.Events.AsNoTracking().SingleAsync(e => e.Id == origin.Id);
        delete.Events.Remove(deleting);
        var deleteCount = await delete.SaveChangesAsync();
    
        // Then
        await using var verify = CreateDbContext<AppDbContext>();
        var deleted = await verify.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == origin.Id);

        Assert.Equal(1, deleteCount);
        Assert.Null(deleted);
        Assert.Equal(infoOrigin, deleting.ToOutputData());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task InsertBooking_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();

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
    
        // When
        await using var work = CreateDbContext<AppDbContext>();
        work.Events.Add(@event);
        work.Bookings.Add(booking);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext<AppDbContext>();
        var bookingFound = await verify
            .Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .SingleOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent, bookingFound.Event.ToOutputData());
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }

    [Fact]
    public async Task UpdateBooking_Success()
    {
        // Given
        await ResetDatabase<AppDbContext>();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoEvent = @event.ToOutputData();

        var booking = new Booking(@event.Id);

        await using var create = CreateDbContext<AppDbContext>();
        create.Events.Add(@event);
        create.Bookings.Add(booking);
        await create.SaveChangesAsync();

        // When
        await using var work = CreateDbContext<AppDbContext>();
        var updating = await work.Bookings.AsNoTracking().SingleAsync(b => b.Id == booking.Id);
        updating.Confirm();
        work.Bookings.Update(updating);
        var updateCount = await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext<AppDbContext>();
        var updated = await verify
            .Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .SingleOrDefaultAsync(b => b.Id == booking.Id);

        Assert.Equal(1, updateCount);
        Assert.NotNull(updated?.Event);
        Assert.Equal(updating.ToInfo(), updated.ToInfo());
        Assert.Equal(infoEvent, updated.Event.ToOutputData());
    }
}
