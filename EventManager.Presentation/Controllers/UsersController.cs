using EventManager.Application.Authorization;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

[ApiController]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost("/auth/register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    //[Authorize(Policy = Policies.UserService.RegisterUser)]
    [AllowAnonymous]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellation)
    {
        await userService.RegisterUserAsync(request, cancellation);
        return NoContent();
    }

    [HttpPut("/auth/{login}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.ModifyUser)]
    public async Task<ActionResult> ChangeRole(
        [FromRoute] string login,
        [FromQuery] string role,
        CancellationToken cancellation)
    {
        var request = new RegisterUserRequest()
            .SetLogin(login)
            .ChangeRole(role);

        await userService.ChangeRoleAsync(request, cancellation);
        return NoContent();
    }

    [HttpPut("/auth/{login}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.ChangePassword)]
    public async Task<ActionResult> ChangePassword(
        [FromRoute] string login,
        [FromBody] string? password,
        CancellationToken cancellation)
    {
        var request = new RegisterUserRequest()
            .SetLogin(login)
            .SetPassword(password);

        await userService.ChangePasswordAsync(request, cancellation);
        return NoContent();
    }

    [HttpDelete("/auth/{login}/unregister")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.DeleteUser)]
    public async Task<ActionResult> Unregister([FromRoute] string login, CancellationToken cancellation)
    {
        var request = new RegisterUserRequest().SetLogin(login);
        await userService.DeleteUserAsync(request, cancellation);
        return NoContent();
    }
}