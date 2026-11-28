namespace Common.Contracts.Kafka;

public enum BookingAction
{
    Confirm,
    Cancel
}

public record BookingActionMessage(
    Guid BookingId,
    Guid EventId,
    int Seats,
    Guid UserId,
    DateTime CreatedAt,
    BookingAction Action
);
