using EventManager.Application.Authorization;
using EventManager.Application.DataTransferObjects;
using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.Presentation.Controllers;

[ApiController]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost("/auth/register")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.UserService.RegisterUser)]
    public async Task<ActionResult<UserInfo>> Register([FromBody] RegisterUserRequest request, CancellationToken cancellation)
        => await userService.RegisterUserAsync(request, cancellation);

    [HttpPut("/auth/register")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.ModifyUser)]
    public async Task<ActionResult<UserInfo>> ChangeRole([FromBody] RegisterUserRequest request, CancellationToken cancellation)
        => await userService.ChangeRoleAsync(request, cancellation);

    [HttpPut("/auth/password")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.ChangePassword)]
    public async Task<ActionResult<UserInfo>> ChangePassword(UserRequest request, CancellationToken cancellation)
        => await userService.ChangePasswordAsync(request, cancellation);

    [HttpDelete("/auth/register")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [Authorize(Policy = Policies.UserService.DeleteUser)]
    public async Task<ActionResult<UserInfo>> Delete(UserRequest request, CancellationToken cancellation)
        => await userService.DeleteUserAsync(request, cancellation);
}