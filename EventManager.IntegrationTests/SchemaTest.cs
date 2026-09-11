using System.Reflection.Metadata;
using EventManager.Application.DataTransfer;
using EventManager.Data;
using EventManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace EventManager.IntegrationTests;

public class SchemaTest : DatabaseTestBase<AppDbContext>
{
    [Trait(Category, Category_Database)]
    [Fact]
    public async Task DatabaseConnect_Success()
    {
        // Given
        await ResetDatabase();
    
        // When
        await using var context = CreateDbContext();
    
        // Then
        Assert.True(context.Database.CanConnect());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task InsertEvent_Success()
    {
        // Given
        await ResetDatabase();

        Event eventToAdd = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        var infoToAdd = eventToAdd.ToOutputData();

        // When
        await using var work = CreateDbContext();
        work.Events.Add(eventToAdd);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
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
        await ResetDatabase();

        Event origin = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        
        await using var context = CreateDbContext();
        context.Events.Add(origin);
        await context.SaveChangesAsync();

        // When
        await using var work = CreateDbContext();
        var modified = await work.Events.AsNoTracking().SingleAsync(e => e.Id == origin.Id);
        modified.Title = "New Title";
        modified.Description = "New Description";
        modified.StartAt = origin.StartAt.AddHours(2);
        modified.EndAt = DateTime.UtcNow.AddHours(2);
        work.Events.Update(modified);
        var updateCount = await work.SaveChangesAsync();
        var infoModified = modified.ToOutputData();
    
        // Then
        await using var verify = CreateDbContext();
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
        await ResetDatabase();

        Event @event = new(10)
        {
            Title = "Event",
            Description = "Description",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        };
        
        await using var context = CreateDbContext();
        context.Events.Add(@event);
        await context.SaveChangesAsync();
        var info = @event.ToOutputData();

        // When
        await using var work = CreateDbContext();
        var deleting = await work.Events.AsNoTracking().SingleAsync(e => e.Id == info.Id);
        work.Events.Remove(deleting);
        var deleteCount = await work.SaveChangesAsync();
    
        // Then
        await using var verify = CreateDbContext();
        var deleted = await verify.Events.AsNoTracking().SingleOrDefaultAsync(e => e.Id == @event.Id);

        Assert.Equal(1, deleteCount);
        Assert.Null(deleted);
        Assert.Equal(info, deleting.ToOutputData());
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task InsertBooking_Success()
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
    
        // When
        await using var work = CreateDbContext();
        work.Events.Add(@event);
        work.Bookings.Add(booking);
        await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
        var bookingFound = await verify
            .Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .SingleOrDefaultAsync(b => b.Id == booking.Id);

        Assert.NotNull(bookingFound?.Event);
        Assert.Equal(infoEvent, bookingFound.Event.ToOutputData());
        Assert.Equal(infoBooking, bookingFound.ToInfo());
    }

    [Trait(Category, Category_Database)]
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
        await using var work = CreateDbContext();
        var updating = await work.Bookings.AsNoTracking().SingleAsync(b => b.Id == booking.Id);
        updating.Confirm();
        work.Bookings.Update(updating);
        var updateCount = await work.SaveChangesAsync();

        // Then
        await using var verify = CreateDbContext();
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

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task EventsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var id = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;
    
        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
                VALUES({id}, {title}, {startAt}, {endAt}, {totalSeats})
            "));
        
        Assert.Equal(PostgresErrorCodes.UniqueViolation, ex.SqlState);
    }

    [Trait(Category, Category_Database)]
    [Fact]
    public async Task BookingsPrimaryKeyTest()
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        var db = context.Database;
        var eventId = Guid.NewGuid();
        var title = "Simple event";
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddHours(10);
        var totalSeats = 10;
        var bookingId = Guid.NewGuid();
        var status = BookingStatus.Pending;
        var createdAt = DateTime.UtcNow;
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Events""(""Id"", ""Title"", ""StartAt"", ""EndAt"", ""TotalSeats"") 
            VALUES({eventId}, {title}, {startAt}, {endAt}, {totalSeats})
        ");

        // When
        await db.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""Bookings""(""Id"", ""EventId"", ""Status"", ""CreatedAt"")
            VALUES({bookingId}, {eventId}, {status}, {createdAt})
        ");

        // Then
        var ex = await Assert.ThrowsAnyAsync<PostgresException>(
            async () => await db.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Bookings""(""Id"", ""EventId"", ""Status"", ""CreatedAt"")
                VALUES({bookingId}, {eventId}, {status}, {createdAt})
            "));
        Assert.Equal(PostgresErrorCodes.UniqueViolation, ex.SqlState);
    }

    /*
    SELECT "Id",
       "Title",
       "Description",
       "StartAt",
       "EndAt",
       "TotalSeats",
       "ReservedCount"
FROM public."Events"
LIMIT 1000;
    23505

    SELECT "Id",
       "EventId",
       "Status",
       "CreatedAt",
       "ProcessedAt"
FROM public."Bookings"
LIMIT 1000;
    */
}
