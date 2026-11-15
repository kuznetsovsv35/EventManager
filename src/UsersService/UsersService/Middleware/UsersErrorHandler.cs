using Common.Middleware.ErrorHandling;
using Microsoft.AspNetCore.Mvc;
using UsersService.Domain.Exceptions;

namespace UsersService.Middleware;

public class UsersProblemDetailsMapper : IProblemDetailsMapper
{
    public ProblemDetails? MapException(Exception exception)
        => exception switch
        {
            UserLoginException loginUser => ProblemDetailsFactory.Unauthorized(loginUser.Message),
            UserRegisterException registerUser => ProblemDetailsFactory.BadRequest(registerUser.InnerException?.Message, registerUser.Message),
            UserNotFoundException userNotFound => ProblemDetailsFactory.NotFound($"{userNotFound.Message}: {userNotFound.Login}."),

            _ => null
        };
}
