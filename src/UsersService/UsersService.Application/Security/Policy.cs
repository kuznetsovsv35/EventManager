namespace UsersService.Application.Security;

public static class Policy
{
        public const string ChangePassword = nameof(ChangePassword);
        public const string ModifyUser = nameof(ModifyUser);
        public const string DeleteUser = nameof(DeleteUser);
        public const string RegisterUser = nameof(RegisterUser);
}
