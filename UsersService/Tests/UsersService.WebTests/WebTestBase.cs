using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using UsersService.Application.DTO;

namespace UsersService.WebTests;

public abstract class WebTestBase(WebTestFactory factory) : IClassFixture<WebTestFactory>
{
    protected async Task<HttpResponseMessage> PostAsync<TPayload>(string uri, TPayload? payload, string? token = null) where TPayload : class
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.PostAsync(
            uri,
            payload is not null
                ? new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, MediaTypeNames.Application.Json)
                : null,
            CancellationToken.None);
    }

    protected async Task<HttpResponseMessage> PutAsync<TPayload>(string uri, TPayload payload, string? token = null) where TPayload : class
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.PutAsJsonAsync(uri, payload, CancellationToken.None);
    }

    protected async Task<HttpResponseMessage> GetAsync(string uri, string? token = null)
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.GetAsync(uri, CancellationToken.None);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string uri, string? token = null)
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.DeleteAsync(uri, CancellationToken.None);
    }

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
        return await response.Content.ReadFromJsonAsync<AuthResult>();
    }
}
