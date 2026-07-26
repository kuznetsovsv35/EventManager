using System.ComponentModel.DataAnnotations;
using EventManager.Application.DataTransfer;
using EventManager.Models;

namespace EventManager.Tests;

public class DataTransferTest : TraitAttributes
{
    public static readonly IEnumerable<object[]> ValidEventInputData = [
        [new EventInputData() { Title = "First Event Title", StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24), TotalSeats = 10}],
        [new EventInputData() { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 02, 23), TotalSeats = 20}],
        [new EventInputData() { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23), TotalSeats = 30}]
    ];
    
    [Trait(Category, Category_DTO)]
    [Theory]
    [MemberData(nameof(ValidEventInputData))]
    public async Task InputData_Event_Success(EventInputData inputData)
    {
        // Given
    
        // When
        var @event = inputData.ToEvent();
    
        // Then
        Assert.Equal(inputData.Title, @event.Title);
        Assert.Equal(inputData.StartAt, @event.StartAt);
        Assert.Equal(inputData.EndAt, @event.EndAt);
        Assert.Equal(inputData.TotalSeats, @event.TotalSeats);
    }

    public static readonly IEnumerable<object[]> InvalidEventInputData = [
        [new EventInputData() { Title = null, StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24), TotalSeats = 10}, nameof(EventInputData.Title)],
        [new EventInputData() { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 01, 23), TotalSeats = 20}, nameof(EventInputData.StartAt), nameof(EventInputData.EndAt)],
        [new EventInputData() { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23), TotalSeats = 0}, nameof(EventInputData.TotalSeats)]
    ];

    [Trait(Category, Category_DTO)]
    [Theory]
    [MemberData(nameof(InvalidEventInputData))]
    public async Task InputData_Event_Fail(EventInputData inputData, params string[] expectedMembers)
    {
        // Given
    
        // When
    
        // Then
        var ex = Assert.Throws<ValidationException>(() => _ = inputData.ToEvent());
        Assert.NotNull(ex?.ValidationResult?.MemberNames);
        var result = expectedMembers.Intersect(ex.ValidationResult.MemberNames);
        Assert.NotEmpty(result);
    }

    public static readonly IEnumerable<object[]> ValidEventIs = [
        [new Event(10) { Title = "First Event Title", StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24)}],
        [new Event(20) { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 02, 23)}],
        [new Event(30) { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23)}]
    ];

    [Trait(Category, Category_DTO)]
    [Theory]
    [MemberData(nameof(ValidEventIs))]
    public async Task Event_OutputData_Success(Event @event)
    {
        // Given
    
        // When
        var outputData = @event.ToOutputData();
    
        // Then
        Assert.Equal(@event.Title, outputData.Title);
        Assert.Equal(@event.StartAt, outputData.StartAt);
        Assert.Equal(@event.EndAt, outputData.EndAt);
        Assert.Equal(@event.TotalSeats, outputData.TotalSeats);
    }

    public static readonly IEnumerable<object[]> Bookings = [
        [new Booking(Guid.NewGuid())],
        [new Booking(Guid.NewGuid())],
        [new Booking(Guid.NewGuid())],
    ];

    [Trait(Category, Category_DTO)]
    [Theory]
    [MemberData(nameof(Bookings))]
    public async Task Booking_Info_Success(Booking booking)
    {
        // Given
        var expectedStatus = Random.Shared.Next(0, 1) % 2 == 0
            ? BookingStatus.Confirmed
            : BookingStatus.Rejected;

        // When
        switch(expectedStatus)
        {
            case BookingStatus.Confirmed:
                booking.Confirm();
                break;
            case BookingStatus.Rejected:
                booking.Reject();
                break;
        }
        
        var info = booking.ToInfo();
    
        // Then
        Assert.Equal(booking.Id, info.Id);
        Assert.Equal(booking.EventId, info.EventId);
        Assert.Equal(booking.Status, info.Status);
        Assert.Equal(booking.ProcessedAt, info.ProcessedAt);
        Assert.Equal(expectedStatus, info.Status);
    }

}