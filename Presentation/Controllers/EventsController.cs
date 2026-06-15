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
    [ProducesResponseType(typeof(IEnumerable<EventOutputData>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventOutputData>> GetEvents() => Ok(eventService.GetAllEvents());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> GetEvent(Guid id)
    {
        if (eventService.GetEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpPost]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> PostEvent([FromBody] EventInputData inputData)
    {
        var outputData = eventService.CreateEvent(inputData);
        return CreatedAtAction(
            nameof(GetEvent), 
            new { id = outputData.Id },
            outputData);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> UpdateEvent(Guid id, [FromBody] EventInputData data)
    {
        if (eventService.UpdateEvent(id, data) is EventOutputData result)
            return Ok(result);
        return NotFound($"Событие id={id} не найдено.");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(EventOutputData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> DeleteEvent(Guid id)
    {
        if (eventService.DeleteEvent(id) is EventOutputData data)
            return Ok(data);
        return NotFound($"Событие id={id} не найдено.");
    }
}