namespace EventManager.Application.DataTransfer;

/// <summary>
/// Выходные данные запросов GET.
/// </summary>
/// <param name="source"></param>
public class EventOutputData : EventInputData
{
    public Guid Id { get; internal init; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is EventOutputData outputData)
            return Id == outputData.Id && base.Equals((EventInputData)obj);

        return base.Equals(obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);
}