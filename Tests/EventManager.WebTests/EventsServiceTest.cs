using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventManager.Application.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

namespace EventManager.WebTests;

public class EventsServiceTest(WebTestFactory factory) : WebTestBase(factory)
{
    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Post_Success()
    {
        // Given
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
    
        // When
        var response = await PostAsync("/events", eventInputData, accessToken);
        EventOutputData? eventOutputData = await response.Content.ReadFromJsonAsync<EventOutputData>();
    
        // Then
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(eventOutputData);
        Assert.Equal(eventInputData, eventOutputData);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Post_BadRequest()
    {
        // Given
        DateTime startAt = DateTime.UtcNow;
        EventInputData eventInputData = new()
        {
            Title = null,
            StartAt = startAt,
            EndAt = startAt.AddHours(-1),
            TotalSeats = 0
        };
        string accessToken = await LoginUser("admin", "admin");
        IEnumerable<string> expectedErrors = new List<string>
        {
            nameof(eventInputData.Title), 
            nameof(eventInputData.StartAt),
            nameof(eventInputData.EndAt),
            nameof(eventInputData.TotalSeats)
        }.Order();
    
        // When
        var response = await PostAsync("/events", eventInputData, accessToken);
        ValidationProblemDetails? problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        IEnumerable<string>? actualErrors = problemDetails?.Errors.Keys.Order();
    
        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.NotNull(actualErrors);
        Assert.Equal(expectedErrors, actualErrors);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Event_Post_Unauthorized()
    {
        // Given
        string eventTitle = $"Event {Guid.NewGuid()}";
        DateTime startAt = DateTime.UtcNow;
        EventInputData eventInputData = new()
        {
            Title = eventTitle,
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            TotalSeats = 50
        };
    
        // When
        var response = await PostAsync("/events", eventInputData);
    
        // Then
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Event_Post_Forbidden()
    {
        // Given
        string eventTitle = $"Event {Guid.NewGuid()}";
        DateTime startAt = DateTime.UtcNow;
        EventInputData eventInputData = new()
        {
            Title = eventTitle,
            StartAt = startAt,
            EndAt = startAt.AddHours(1),
            TotalSeats = 50
        };
        string accessToken = await LoginUser("user", "user");
    
        // When
        var response = await PostAsync("/events", eventInputData, accessToken);
    
        // Then
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_GetById_Success()
    {
        // Given
        EventOutputData? eventInfo = await CreateRandomEvent();
        string accessToken = await LoginUser("user", "user");
    
        // When
        var response = eventInfo is not null 
            ? await GetAsync($"/events/{eventInfo.Id}", accessToken)
            : null;

        response?.EnsureSuccessStatusCode();

        EventOutputData? eventFound = response is not null 
            ? await response.Content.ReadFromJsonAsync<EventOutputData>()
            : null;
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);        
        Assert.Equal(eventInfo, eventFound);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_GetById_NotFound()
    {
        // Given
        string accessToken = await LoginUser("user", "user");
    
        // When
        var response = await GetAsync($"/events/{Guid.NewGuid()}", accessToken);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);        
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_GetById_Unauthorized()
    {
         // Given
        EventOutputData? eventInfo = await CreateRandomEvent();
    
        // When
        var response = eventInfo is not null 
            ? await GetAsync($"/events/{eventInfo.Id}")
            : null;
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);        
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Put_Success()
    {
        // Given
        string accessToken = await LoginUser("admin", "admin");
        EventOutputData original = (await CreateRandomEvent())!;

        DateTime startAt = original.StartAt.AddDays(1);

        EventInputData update = new()
        {
            Description = "some description",
            EndAt = startAt.AddHours(2),
            StartAt = startAt,
            Title = original.Title + " (modify)",
            TotalSeats = 150,
        };
    
        // When
        var response = await PutAsync($"/events/{original.Id}", update, accessToken);

        response?.EnsureSuccessStatusCode();

        EventOutputData? modified = response is not null
            ? await response.Content.ReadFromJsonAsync<EventOutputData>()
            : null;
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(update, modified);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Put_BadRequest()
    {
        // Given
        string accessToken = await LoginUser("admin", "admin");
        EventOutputData original = (await CreateRandomEvent())!;

        DateTime startAt = original.StartAt.AddDays(1);

        EventInputData update = new()
        {
            StartAt = startAt.AddHours(2),
            EndAt = startAt,
            Title = null,
            TotalSeats = 0,
        };

        IEnumerable<string> expectedErrors = new List<string>
        {
            nameof(update.Title), 
            nameof(update.StartAt),
            nameof(update.EndAt),
            nameof(update.TotalSeats)
        }.Order();
    
        // When
        var response = await PutAsync($"/events/{original.Id}", update, accessToken);

        ValidationProblemDetails? problemDetails = response is not null
            ? await response.Content.ReadFromJsonAsync<ValidationProblemDetails>()
            : null;

        IEnumerable<string>? actualErrors = problemDetails?.Errors.Keys.Order();
   
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.NotNull(problemDetails.Errors);
        Assert.Equal(expectedErrors, actualErrors);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Put_NotFound()
    {
        // Given
        string accessToken = await LoginUser("admin", "admin");
        EventOutputData original = (await CreateRandomEvent())!;

        DateTime startAt = original.StartAt.AddDays(1);

        EventInputData update = new()
        {
            Description = "some description",
            EndAt = startAt.AddHours(2),
            StartAt = startAt,
            Title = original.Title + " (modify)",
            TotalSeats = 150,
        };
    
        // When
        var response = await PutAsync($"/events/{Guid.Empty}", update, accessToken);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Put_Forbidden()
    {
        // Given
        string accessToken = await LoginUser("user", "user");
        EventOutputData original = (await CreateRandomEvent())!;

        DateTime startAt = original.StartAt.AddDays(1);

        EventInputData update = new()
        {
            Description = "some description",
            EndAt = startAt.AddHours(2),
            StartAt = startAt,
            Title = original.Title + " (modify)",
            TotalSeats = 150,
        };
    
        // When
        var response = await PutAsync($"/events/{original.Id}", update, accessToken);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Put_Unauthorized()
    {
        // Given
        EventOutputData original = (await CreateRandomEvent())!;

        DateTime startAt = original.StartAt.AddDays(1);

        EventInputData update = new()
        {
            Description = "some description",
            EndAt = startAt.AddHours(2),
            StartAt = startAt,
            Title = original.Title + " (modify)",
            TotalSeats = 150,
        };
    
        // When
        var response = await PutAsync($"/events/{original.Id}", update);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Delete_Success()
    {
        // Given
        EventOutputData original = (await CreateRandomEvent())!;
        string accessToken = await LoginUser("admin", "admin");
    
        // When
        var response = await DeleteAsync($"/events/{original.Id}", accessToken);
        response?.EnsureSuccessStatusCode();
        EventOutputData? deleted = response is not null
            ? await response.Content.ReadFromJsonAsync<EventOutputData>()
            : null;
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(deleted);
        Assert.Equal(original, deleted);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Delete_NotFound()
    {
        // Given
        string accessToken = await LoginUser("admin", "admin");
    
        // When
        var response = await DeleteAsync($"/events/{Guid.Empty}", accessToken);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Delete_Unauthorized()
    {
        // Given
        EventOutputData original = (await CreateRandomEvent())!;
    
        // When
        var response = await DeleteAsync($"/events/{original.Id}");
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Events_Delete_Forbidden()
    {
        // Given
        EventOutputData original = (await CreateRandomEvent())!;
        string accessToken = await LoginUser("user", "user");
    
        // When
        var response = await DeleteAsync($"/events/{original.Id}", accessToken);
    
        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}