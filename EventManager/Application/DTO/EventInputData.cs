namespace EventManager.Application.DataTransfer;

/// <summary>
/// Входные данные запроса создания и обновления события.
/// </summary>
[EventInputDataValidation]
public class EventInputData()
{
    [EventInputDataValidation("Заголовок события не может быть пустым.")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [EventInputDataValidation("Момент начала события не может быть позже момента окончания.")]
    public DateTime StartAt { get; set; }

    [EventInputDataValidation("Момент окончания события не может быть раньше момента начала.")]
    public DateTime EndAt { get; set; }

    [EventInputDataValidation("Общее число мест должно быть положительным числом")]
    public int TotalSeats { get; set; }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is EventInputData inputData)
            return Title == inputData.Title
                && StartAt.ToString() == inputData.StartAt.ToString()
                && EndAt.ToString() == inputData.EndAt.ToString()
                && Description == inputData.Description
                && TotalSeats == inputData.TotalSeats;

        return base.Equals(obj);
    }

    public override int GetHashCode()
        => HashCode.Combine(Title ?? string.Empty, StartAt, EndAt);
}
