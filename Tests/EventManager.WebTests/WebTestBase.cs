using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EventManager.Application.DataTransferObjects;
using EventManager.Infrastructure.Authentication;

namespace EventManager.WebTests;

public abstract class WebTestBase(WebTestFactory factory) : IClassFixture<WebTestFactory> 
{
    protected const string Category = nameof(Category);
    protected const string Category_Web = "web";
    
    protected async Task<HttpResponseMessage> PostAsync<TPayload>(string uri, TPayload? payload, string? token = null) where TPayload : class
    {
        using var client = factory.CreateClient();
        
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return await client.PostAsync(
            uri, 
            payload is not null 
                ? new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, MediaTypeNames.Application.Json) : null, 
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

    protected async Task<string> LoginUser(string login, string? password)
    {
        UserRequest request = new(){ Login = login, Password = password };
        var response = await PostAsync("/auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>();
        Assert.NotNull(result);
        return result.AccessToken;
    }

    protected async Task<EventOutputData?> CreateRandomEvent()
    {
        string eventTitle = $"Event {Guid.NewGuid()}";
        DateTime startAt = DateTime.UtcNow;
        EventInputData eventInputData = new()
        {
            Title = eventTitle,
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            TotalSeats = 50
        };
        string accessToken = await LoginUser("admin", "admin");
    
        var response = await PostAsync("/events", eventInputData, accessToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EventOutputData>();
    }
}