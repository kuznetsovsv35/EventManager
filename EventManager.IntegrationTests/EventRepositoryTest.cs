using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Data;
using EventManager.Models;

namespace EventManager.IntegrationTests;

public class EventRepositoryTest : DatabaseTestBase<AppDbContext>
{
    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task AddNewEvent_Success()
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
        var info = @event.ToOutputData();

        // When
        await using var workContext = CreateDbContext();
        IEventRepository workRepository = new EventRepository(workContext);
        await workRepository.AddEventAsync(@event, CancellationToken.None);

        // Then
        await using var verifyContext = CreateDbContext();
        IEventRepository verifyRepository = new EventRepository(verifyContext);
        var eventAdded = await verifyRepository.GetEventAsync(@event.Id, CancellationToken.None);

        Assert.NotNull(eventAdded);
        Assert.Equal(info, eventAdded.ToOutputData());
    }

    [Trait(Category, Category_Repositories)]
    [Fact]
    public async Task UpdateEvent_Success()
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

        // When
        await using var workContext = CreateDbContext();
        IEventRepository workRepository = new EventRepository(workContext);
        Event modified = (await workRepository.GetEventAsync(@event.Id, CancellationToken.None))!;
        modified.Title = "New Title";
        modified.Description = "New Description";
        modified.StartAt = @event.StartAt.AddHours(2);
        modified.EndAt = @event.EndAt.AddHours(2);
        await workRepository.UpdateEventAsync(modified, CancellationToken.None);
        var infoModified = modified.ToOutputData();
    
        // Then
        await using var verifyContext = CreateDbContext();
        IEventRepository verifyRepository = new EventRepository(verifyContext);
        var updated = await verifyRepository.GetEventAsync(@event.Id, CancellationToken.None);
        var infoUpdated = updated?.ToOutputData();

        Assert.NotNull(infoUpdated);
        Assert.Equal(infoModified, infoUpdated);
    }

    [Trait(Category, Category_Repositories)]
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
        await using var workContext = CreateDbContext();
        IEventRepository workRepository = new EventRepository(workContext);
        var deleting = await workRepository.GetEventAsync(@event.Id, CancellationToken.None)!;
        var deleted = await workRepository.DeleteEventAsync(@event.Id, CancellationToken.None);
    
        // Then
        await using var verifyContext = CreateDbContext();
        IEventRepository verifyRepository = new EventRepository(verifyContext);
        var found = await verifyRepository.GetEventAsync(@event.Id, CancellationToken.None);

        Assert.Null(found);
        Assert.Equal(info, deleting?.ToOutputData());
    }
}