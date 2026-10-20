using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EventManager.Application.DataTransferObjects;
using EventManager.Domain.ValueObjects;
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
        RegisterUserRequest request = new()
        {
            Login = login,
            Password = password,
            Role = UserRole.User,
        };
        var response = await PostAsync("auth/register", request);
        response.EnsureSuccessStatusCode();
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