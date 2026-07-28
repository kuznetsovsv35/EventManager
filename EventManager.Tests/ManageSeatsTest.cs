using EventManager.Models;

namespace EventManager.Tests;

/// <summary>
/// Тесты логики управления местами на событиях.
/// </summary>
public class ManageSeatsTest : TestObjectBase
{
    public static readonly IEnumerable<object[]> ReserveSeats_Data = [
        [new Event(10) { Title = "First Event Title", StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24)}],
        [new Event(20) { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 02, 23)}],
        [new Event(30) { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23)}],
    ];

    [Trait(Category, Category_Seats)]
    [Theory]
    [MemberData(nameof(ReserveSeats_Data))]
    public async Task ReserveSeats_Success(Event @event)
    {
        // Given
        var totalSeats = @event.TotalSeats;
        var reserveCount = @event.AvailableSeats / 2;
        var expectedAvailableSeats = @event.AvailableSeats - reserveCount;

        // When
        var reserveResult = @event.TryReserveSeats(reserveCount);

        // Then
        Assert.True(reserveResult);
        Assert.Equal(totalSeats, @event.TotalSeats);
        Assert.Equal(expectedAvailableSeats, @event.AvailableSeats);
    }

    [Trait(Category, Category_Seats)]
    [Theory]
    [MemberData(nameof(ReserveSeats_Data))]
    public async Task ReserveSeats_Fail(Event @event)
    {
        // Given
        var totalSeats = @event.TotalSeats;
        var reserveCount = @event.AvailableSeats + 1;
        var expectedAvailableSeats = @event.AvailableSeats;

        // When
        var reserveResult = @event.TryReserveSeats(reserveCount);

        // Then
        Assert.False(reserveResult);
        Assert.Equal(totalSeats, @event.TotalSeats);
        Assert.Equal(expectedAvailableSeats, @event.AvailableSeats);
    }

    [Trait(Category, Category_Seats)]
    [Theory]
    [MemberData(nameof(ReserveSeats_Data))]
    public async Task ReserveReleaseSets_Success(Event @event)
    {
        // Given
        var totalSeats = @event.TotalSeats;
        var reserveCount = Random.Shared.Next(1, @event.AvailableSeats + 1);
        var releaseCount = Random.Shared.Next(1, reserveCount + 1);
        var expectedAfterReserve = @event.AvailableSeats - reserveCount;
        var expectedAfterRelease = expectedAfterReserve + releaseCount;

        // When
        var reserveResult = @event.TryReserveSeats(reserveCount);
        var totalAfterReserve = @event.TotalSeats;
        var availableAfterReserve = @event.AvailableSeats;

        @event.ReleaseSeats(releaseCount);
        var totalAfterRelease = @event.TotalSeats;
        var availableAfterRelease = @event.AvailableSeats;

        // Then
        Assert.True(reserveResult);
        Assert.Equal(totalSeats, totalAfterReserve);
        Assert.Equal(expectedAfterReserve, availableAfterReserve);

        Assert.Equal(totalSeats, totalAfterRelease);
        Assert.Equal(expectedAfterRelease, availableAfterRelease);
    }

    public static readonly IEnumerable<object[]> UpdateSeats_Data = [
        [new Event(10) { Title = "First Event Title", StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24)}],
        [new Event(20) { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 02, 23)}],
        [new Event(30) { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23)}],
    ];

    [Trait(Category, Category_Seats)]
    [Theory]
    [MemberData(nameof(UpdateSeats_Data))]
    public async Task UpdateTotalSeats_Success(Event @event)
    {
        // Given
        var totalSeats = @event.TotalSeats;
        var reserveCount = Random.Shared.Next(1, @event.AvailableSeats + 1);
        var newTotalSeats = Random.Shared.Next(1, totalSeats + 1);

        // When
        var reserveResult = @event.TryReserveSeats(reserveCount);
        var availableSeats = @event.AvailableSeats;
        @event.UpdateTotalSeats(newTotalSeats);

        // Then
        Assert.True(reserveResult);

        if (newTotalSeats < reserveCount)
        {
            Assert.Equal(reserveCount, @event.TotalSeats);
            Assert.Equal(0, @event.AvailableSeats);
        }
        else
        {
            Assert.Equal(newTotalSeats, @event.TotalSeats);
            Assert.Equal(newTotalSeats - totalSeats, @event.AvailableSeats - availableSeats);
        }
    }
}