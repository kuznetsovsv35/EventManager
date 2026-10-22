using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using EventManager.Application.Authorization;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// API контроллер управления событиями CRUD.
/// </summary>
/// <param name="eventService"></param>
[ApiController]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet("/events")]
    [ProducesResponseType<PaginateResult<EventOutputData>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Policy = Policies.EventService.GetEvents)]
    public async Task<ActionResult<PaginateResult<EventOutputData>>> GetEvents(
        [FromQuery] EventQueryParams queryParams, CancellationToken cancellation)
            => Ok(await eventService.GetEvents(queryParams, queryParams, cancellation));

    [HttpGet("/events/{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Policy = Policies.EventService.GetEvents)]
    public async Task<ActionResult<EventOutputData>> GetEvent(Guid id, CancellationToken cancellation)
        => Ok(await eventService.GetEventAsync(id, cancellation));

    [HttpPost("/events")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = Policies.EventService.CreateEvent)]
    public async Task<ActionResult<EventOutputData>> PostEvent([FromBody] EventInputData inputData, CancellationToken cancellation)
    {
        var e = await eventService.CreateEventAsync(inputData, cancellation);
        return CreatedAtAction(
            nameof(GetEvent),
            new { id = e.Id },
            e);
    }

    [HttpPut("/events/{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = Policies.EventService.ModifyEvent)]
    public async Task<ActionResult<EventOutputData>> UpdateEvent(Guid id, [FromBody] EventInputData data, CancellationToken cancellation)
        => Ok(await eventService.UpdateEventAsync(id, data, cancellation));

    [HttpDelete("/events/{id:guid}")]
    [ProducesResponseType<EventOutputData>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = Policies.EventService.DeleteEvent)]
    public async Task<ActionResult<EventOutputData>> DeleteEvent(Guid id, CancellationToken cancellation)
        => Ok(await eventService.DeleteEventAsync(id, cancellation));
}