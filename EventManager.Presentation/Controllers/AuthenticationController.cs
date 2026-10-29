using Microsoft.AspNetCore.Mvc;
using EventManager.Infrastructure.Authentication;
using EventManager.Application.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EventManager.Presentation.Controllers;

/// <summary>
/// Контролер аутентификации.
/// </summary>
/// <param name="authentication"></param>
[ApiController]
public class AuthenticationController(IAuthenticationService authentication) : ControllerBase
{
    [HttpPost("/auth/login")]
    [ProducesResponseType<AuthResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login([FromBody] UserRequest request, CancellationToken cancellation)
        => await authentication.LoginAsync(request, cancellation);
}