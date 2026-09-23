namespace EventManager.Domain.ValueObjects;

/// <summary>
/// Модель данных события.
/// </summary>
public class Event
{
    /// <summary>
    /// Идентификатор события.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Заголовок описания события.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Дополнительное описание события.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата/время начала события.
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата/время окончания события.
    /// </summary>
    public required DateTime EndAt { get; set; }

    /// <summary>
    /// Общее число мест на событие
    /// </summary>
    public int TotalSeats { get; private set; }

    /// <summary>
    /// Число зарезервированных мест.
    /// </summary>
    public int ReservedSeats { get; private set; }

    /// <summary>
    /// Число доступных мест.
    /// </summary>
    public int AvailableSeats => TotalSeats - ReservedSeats;

    /// <summary>
    /// Попытка забронировать места.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
            return false;

        ReservedSeats += count;
        return true;
    }

    /// <summary>
    /// Освободить места.
    /// </summary>
    /// <param name="count"></param>
    public void ReleaseSeats(int count = 1) => ReservedSeats -= Math.Min(ReservedSeats, count);

    /// <summary>
    /// Обновить общее количество мест.
    /// </summary>
    /// <param name="totalSeats"></param>
    public void UpdateTotalSeats(int totalSeats) => TotalSeats = Math.Max(totalSeats, ReservedSeats);

    /// <summary>
    /// Приватный конструктор без параметров.
    /// </summary>
    Event() { }

    /// <summary>
    /// Конструктор события.
    /// </summary>
    /// <param name="totalSeats"></param>
    public Event(int totalSeats) : this()
    {
        Id = Guid.NewGuid();
        TotalSeats = totalSeats;
    }
}