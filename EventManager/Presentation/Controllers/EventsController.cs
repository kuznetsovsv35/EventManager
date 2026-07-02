using EventManager.Application.DataTransfer;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
    [ProducesResponseType<PaginateResult<EventOutputData>>(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<EventOutputData>> GetEvents(
        [FromQuery] EventQueryParams queryParams
        ) => Ok(eventService.GetEvents(queryParams, queryParams));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> GetEvent(Guid id)
        => Ok(eventService.GetEvent(id));

    [HttpPost]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> PostEvent([FromBody] EventInputData inputData)
    {
        var e = eventService.CreateEvent(inputData);
        return CreatedAtAction(
            nameof(GetEvent),
            new { id = e.Id },
            e);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<EventOutputData> UpdateEvent(Guid id, [FromBody] EventInputData data)
        => Ok(eventService.UpdateEvent(id, data));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<EventOutputData> DeleteEvent(Guid id)
        => Ok(eventService.DeleteEvent(id));
}