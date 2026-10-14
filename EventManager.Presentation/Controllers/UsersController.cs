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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = Policies.UserService.RegisterUser)]
    public async Task<ActionResult<UserInfo>> Register([FromBody] RegisterUserRequest request, CancellationToken cancellation)
        => await userService.RegisterUserAsync(request, cancellation);
}