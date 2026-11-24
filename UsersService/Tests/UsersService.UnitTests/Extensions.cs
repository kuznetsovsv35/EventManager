using Authorization.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UsersService.Application.DTO;

namespace UsersService.UnitTests;

public static class Extensions
{
    public static async Task<UserInfo> GetUserInfo(this IServiceProvider services, string login, UserRole role)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        return (await dbContext.Users
            .AsNoTracking()
            .SingleAsync(u => u.Login == login && u.Role == role && u.IsActive))
            .ToInfo();
    }
}
