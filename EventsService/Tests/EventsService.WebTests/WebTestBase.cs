using Authorization.Domain;
using Common.Tests.AspNetCore;
using EventsService.Application.DTO;

namespace EventsService.WebTests;

public abstract class WebTestBase(WebTestFactory factory) : WebTestBase<Program>(factory), IClassFixture<WebTestFactory>
{
    protected async Task<string> LoginUser(string login)
        => login switch
        {
            "admin" => factory.TokenGenerator.GenerateTokens(Guid.Empty, login, UserRole.Admin),
            _ => factory.TokenGenerator.GenerateTokens(Guid.Empty, login, UserRole.User)
        };

    protected async Task<EventOutputData?> CreateEvent(EventInputData data)
    {
        string accessToken = await LoginUser("admin");

        var response = await PostAsync("/events", data, accessToken);
        response.EnsureSuccessStatusCode();
        return await ReadJsonAsync<EventOutputData>(response.Content);
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