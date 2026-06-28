using System.ComponentModel.DataAnnotations;

namespace EventManager.Application.DataTransfer;

/// <summary>
/// Представление параметров запроса.
/// </summary>
public partial class EventQueryParams
{
    /// <summary>
    /// Строка, входящая в заголовок события.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Начало диапазона, просматриваемых событий (начинаются не раньше).
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Конец диапазона, просматриваемых событий (заканчиваются не позже).
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Текущая страница (разбивка результатов по страницам).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть положительным.")]
    public int Page { get; set; } = PageParams.DefaultPageNumber;

    /// <summary>
    /// Количество событий на странице (разбивка по страницам).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Значение должно быть положительным.")]
    public int PageSize { get; set; } = PageParams.DefaultPageSize;
}