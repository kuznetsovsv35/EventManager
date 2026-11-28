using BookingService.Application.DTO;
using BookingService.Domain;
using Common.Tests;

namespace BookingService.UnitTests;

/// <summary>
/// Тесты DTO.
/// </summary>
public class DataTransferTest
{
    public static readonly IEnumerable<object[]> Bookings = [
        [new Booking(Guid.NewGuid(), Guid.Empty)],
        [new Booking(Guid.NewGuid(), Guid.Empty)],
        [new Booking(Guid.NewGuid(), Guid.Empty)],
    ];

    /// <summary>
    /// Тест преобразования сущности брони в выходные данные.
    /// </summary>
    /// <param name="booking"></param>
    /// <returns></returns>
    [Trait(Category.Name, Category.DTO)]
    [Theory]
    [MemberData(nameof(Bookings))]
    public async Task Booking_Info_Success(Booking booking)
    {
        // Given
        var expectedStatus = Random.Shared.Next(0, 2) % 2 == 0
            ? BookingStatus.Confirmed
            : BookingStatus.Rejected;

        // When
        switch (expectedStatus)
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