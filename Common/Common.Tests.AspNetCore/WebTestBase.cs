using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Common.Tests.AspNetCore;

public abstract class WebTestBase<TProgram>(WebApplicationFactory<TProgram> factory) where TProgram : class
{
    protected readonly JsonSerializerOptions _jsonOptions = factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

    protected Task<T?> ReadJsonAsync<T>(HttpContent content)
        => content.ReadFromJsonAsync<T>(_jsonOptions);

    protected HttpContent WriteJson<T>(T payload)
        => new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, MediaTypeNames.Application.Json);

    protected async Task<HttpResponseMessage> PostAsync<TPayload>(string uri, TPayload? payload, string? token = null) where TPayload : class
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.PostAsync(uri, payload is not null ? WriteJson(payload) : null, CancellationToken.None);
    }

    protected async Task<HttpResponseMessage> PutAsync<TPayload>(string uri, TPayload payload, string? token = null) where TPayload : class
    {
        using var client = factory.CreateClient();

        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await client.PutAsync(uri, WriteJson(payload), CancellationToken.None);
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
}
