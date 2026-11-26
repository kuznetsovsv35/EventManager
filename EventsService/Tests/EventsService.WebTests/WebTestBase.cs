using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Authorization.Domain;
using EventsService.Application.DTO;

namespace EventsService.WebTests;

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

    protected async Task<string> LoginUser(string login, string? password)
        => login switch
        { 
            "admin" => factory.TokenGenerator.GenerateTokens(Guid.Empty, login,  UserRole.Admin),
            _ => factory.TokenGenerator.GenerateTokens(Guid.Empty, login,  UserRole.User)
        };

    protected async Task<EventOutputData?> CreateEvent(EventInputData data)
    {
        string accessToken = await LoginUser("admin", "admin");

        var response = await PostAsync("/events", data, accessToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventOutputData>();
    }

    protected Task<EventOutputData?> CreateRandomEvent()
    {
        EventInputData data = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(1))
            .Build();

        return CreateEvent(data);
    }

    protected Task<EventOutputData?> CreatePastEvent()
    {
        EventInputData data = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(-1))
            .Build();

        return CreateEvent(data);
    }
}