using Common.Tests.AspNetCore;
using UsersService.Application.DTO;

namespace UsersService.WebTests;

public abstract class WebTestBase(WebTestFactory factory) : WebTestBase<Program>(factory), IClassFixture<WebTestFactory>
{
    protected async Task CreateUser(string login, string? password)
    {
        UserRequest request = new()
        {
            Login = login,
            Password = password,
        };
        var response = await PostAsync("auth/register", request);
        response.EnsureSuccessStatusCode();
    }

    protected async Task<AuthResult?> LoginUser(string login, string? password)
    {
        UserRequest request = new() { Login = login, Password = password };
        var response = await PostAsync("/auth/login", request);
        response.EnsureSuccessStatusCode();
        return await ReadJsonAsync<AuthResult>(response.Content);
    }
}
