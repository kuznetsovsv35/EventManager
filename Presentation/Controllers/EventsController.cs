using EventManager.Application.DataTransfer;
using EventManager.Application.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Infrastructure.Controllers;

/// <summary>
/// API контроллер управления событиями CRUD.
/// </summary>
/// <param name="eventService"></param>
[ApiController]
[Route("api/[Controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetEvents()
    {
        return Ok(eventService.GetAllEvents());
    }

    [HttpGet("{id}")]
    public IActionResult GetEvent(Guid id)
    {
        if (eventService.GetEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpPost]
    public IActionResult PostEvent([FromBody] EventInputData data)
    {
        eventService.CreateEvent(data);
        return Created();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateEvent(Guid id, [FromBody] EventInputData data)
    {
        if (eventService.UpdateEvent(id, data) is EventOutputData result)
            return Ok(result);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteEvent(Guid id)
    {
        if (eventService.DeleteEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }
}