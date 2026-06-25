namespace EventManager.Application.DataTransfer;

/// <summary>
/// Водные данные запроса создания и обновления события.
/// </summary>
[EventInputDataValidation]
public class EventInputData
{
    [EventInputDataValidation("Заголовок события не может быть пустым.")]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [EventInputDataValidation("Момент начала события не может быть позже момента окончания.")]
    public DateTime StartAt { get; set; }

    [EventInputDataValidation("Момент окончания события не может быть раньше момента начала.")]
    public DateTime EndAt { get; set; }
}
