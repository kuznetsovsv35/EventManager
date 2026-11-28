using System.ComponentModel.DataAnnotations;
using Common.Tests;
using EventsService.Application.DTO;
using EventsService.Domain;

namespace EventsService.UnitTests;

/// <summary>
/// Тесты DTO.
/// </summary>
public class DataTransferTest
{
    public static readonly IEnumerable<object[]> ValidEventInputData = [
        [new EventInputData() { Title = "First Event Title", StartAt = new DateTime(2023, 07, 23), EndAt = new DateTime(2023, 07, 24), TotalSeats = 10}],
        [new EventInputData() { Title = "Second Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2026, 02, 23), TotalSeats = 20}],
        [new EventInputData() { Title = "Third Event Title", StartAt = new DateTime(2026, 01, 23), EndAt = new DateTime(2027, 01, 23), TotalSeats = 30}]
    ];

    /// <summary>
    /// Тест преобразования входных данных в сущность события.
    /// </summary>
    /// <param name="inputData"></param>
    /// <returns></returns>
    [Trait(Category.Name, Category.DTO)]
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

    /// <summary>
    /// Тест валидации входных данных (неудачно).
    /// </summary>
    /// <param name="inputData"></param>
    /// <param name="expectedMembers"></param>
    /// <returns></returns>
    [Trait(Category.Name, Category.DTO)]
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

    /// <summary>
    /// Тест преобразования сущности события в выходные данные.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    [Trait(Category.Name, Category.DTO)]
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

}