using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CustomProblemDetailsFactory = EventManager.Infrastructure.ProblemDetailsFactory;

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
    [ProducesResponseType<IEnumerable<EventOutputData>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventOutputData>> GetEvents(
        [FromQuery] EventQueryParams? queryParams
        ) => Ok(eventService.GetAllEvents());

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> GetEvent(Guid id)
    {
        if (eventService.GetEvent(id) is Event e)
            return Ok(e.ToOutputData());

        return EventNotFound(id);
    }

    [HttpPost]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> PostEvent([FromBody] EventInputData inputData)
    {
        var e = eventService.CreateEvent(inputData);
        return CreatedAtAction(
            nameof(GetEvent),
            new { id = e.Id },
            e.ToOutputData());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> UpdateEvent(Guid id, [FromBody] EventInputData data)
    {
        if (eventService.UpdateEvent(id, data) is Event e)
            return Ok(e.ToOutputData());        
        
        return EventNotFound(id);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> DeleteEvent(Guid id)
    {
        if (eventService.DeleteEvent(id) is Event e)
            return Ok(e.ToOutputData());
        
        return EventNotFound(id);
    }

    NotFoundObjectResult EventNotFound(Guid id)
        => NotFound(CustomProblemDetailsFactory
        .NotFound($"Событие не найдено, ID={id}.")
        .AddInstance(HttpContext.Request.Path).Problem);
}