namespace UsersService.Application.Security;

public static class Policy
{
    public const string ChangePassword = $"{nameof(UsersService)}.{nameof(ChangePassword)}";
    public const string ModifyUser = $"{nameof(UsersService)}.{nameof(ModifyUser)}";
    public const string DeleteUser = $"{nameof(UsersService)}.{nameof(DeleteUser)}";
    public const string RegisterUser = $"{nameof(UsersService)}.{nameof(RegisterUser)}";
}
