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
    [ProducesResponseType<PaginateResult<EventOutputData>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginateResult<EventOutputData>>> GetEvents(
        [FromQuery] EventQueryParams queryParams, CancellationToken cancellation)
            => Ok(await eventService.GetEvents(queryParams, queryParams, cancellation));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventOutputData>> GetEvent(Guid id, CancellationToken cancellation)
        => Ok(await eventService.GetEventAsync(id, cancellation));

    [HttpPost]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventOutputData>> PostEvent([FromBody] EventInputData inputData, CancellationToken cancellation)
    {
        var e = await eventService.CreateEventAsync(inputData, cancellation);
        return CreatedAtAction(
            nameof(GetEvent),
            new { id = e.Id },
            e);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventOutputData>> UpdateEvent(Guid id, [FromBody] EventInputData data, CancellationToken cancellation)
        => Ok(await eventService.UpdateEventAsync(id, data, cancellation));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventOutputData>> DeleteEvent(Guid id, CancellationToken cancellation)
        => Ok(await eventService.DeleteEventAsync(id, cancellation));
}