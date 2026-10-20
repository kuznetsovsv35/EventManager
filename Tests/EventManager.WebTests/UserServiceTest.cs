using System.Net;
using System.Net.Http.Json;
using EventManager.Application.DataTransferObjects;
using EventManager.Domain.ValueObjects;
using EventManager.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace EventManager.WebTests;

public class UserServiceTest(WebTestFactory factory) : WebTestBase(factory)
{

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Auth_Register_Success()
    {
        // Given
        RegisterUserRequest request = new()
        {
            Login = "Test",
            Role = UserRole.User,
            Password = "123",
        };

        // When
        var response = await PostAsync("/auth/register", request);

        // Then
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Auth_Register_InvalidLogin_BadRequest()
    {
        // Given
        RegisterUserRequest request = new()
        {
            Login = string.Empty,
            Password = "123",
            Role = UserRole.User,
        };

        // When
        var response = await PostAsync("/auth/register", request);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Auth_Register_DuplicateLogin_BadRequest()
    {
        // Given
        UserRequest loginRequest = new() { Login = "admin", Password = "admin" };

        RegisterUserRequest registerRequest = new()
        {
            Login = "admin",
            Password = "pass",
            Role = UserRole.Admin,
        };

        // When
        var loginResponse = await PostAsync("auth/login", loginRequest);
        var registerResponse = await PostAsync("/auth/register", registerRequest);

        // Then
        loginResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Theory]
    [InlineData(["admin", "admin"])]
    [InlineData(["user", "user"])]
    public async Task Auth_Login_Success(string login, string password)
    {
        // Given
        UserRequest loginRequest = new()
        {
            Login = login,
            Password = password,
        };

        // When
        var response = await PostAsync("/auth/login", loginRequest);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();

        // Then
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(authResult?.AccessToken);
    }

    [Trait(Category, Category_Web)]
    [Theory]
    [InlineData(["unknown", "pass"])]
    [InlineData(["user", "123"])]
    public async Task Auth_Login_Unauthorized(string login, string password)
    {
        // Given
        UserRequest loginRequest = new()
        {
            Login = login,
            Password = password,
        };

        // When
        var response = await PostAsync("/auth/login", loginRequest);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();

        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(authResult?.AccessToken);
    }
}
