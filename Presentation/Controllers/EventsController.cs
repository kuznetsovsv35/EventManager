using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// API контроллер управления событиями CRUD.
/// </summary>
/// <param name="eventService"></param>
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetEvents()
    {
        return Ok(eventService.GetAllEvents());
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetEvent(Guid id)
    {
        if (eventService.GetEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpPost]
    public IActionResult PostEvent([FromBody] EventInputData inputData)
    {
        var outputData = eventService.CreateEvent(inputData);
        return CreatedAtAction(
            nameof(GetEvent), 
            new { id = outputData.Id },
            outputData);
    }

    [HttpPut("{id:guid}")]
    public IActionResult UpdateEvent(Guid id, [FromBody] EventInputData data)
    {
        if (eventService.UpdateEvent(id, data) is EventOutputData result)
            return Ok(result);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteEvent(Guid id)
    {
        if (eventService.DeleteEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }
}