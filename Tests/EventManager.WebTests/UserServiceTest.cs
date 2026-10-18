using System.Net.Mime;
using System.Text;
using System.Text.Json;
using EventManager.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace EventManager.WebTests;

public class UserServiceTest(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task Test1()
    {
        var request = new { Login = "admin", Password = "admin", Role = UserRole.Admin };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _client.PostAsync("auth/register", content);
    }
}
