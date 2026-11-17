namespace Common.Kafka.Contracts;

public record BookingConfirmed(
    Guid BookingId,
    Guid EventId,
    int ReservedSeats,
    Guid UserId,
    DateTime ConfirmedAt
);
