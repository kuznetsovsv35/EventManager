using EventManager.Domain.ValueObjects;

namespace EventManager.Application.DataTransferObjects;

public class RegisterUserRequest : UserRequest
{
    public UserRole Role { get; set; }
}