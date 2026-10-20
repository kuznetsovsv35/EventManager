using System.Net;
using System.Net.Http.Json;
using EventManager.Application.DataTransferObjects;

namespace EventManager.WebTests;

public class BookingsServiceTest(WebTestFactory factory) : WebTestBase(factory)
{
    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_Success()
    {
        // Given
        Guid eventId = (await CreateRandomEvent())!.Id;
        string accessToken = await LoginUser("user", "user");
    
        // When
        var postResponse = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken);
        postResponse?.EnsureSuccessStatusCode();
        
        BookingInfo? created = postResponse?.Content is not null
            ? await postResponse.Content.ReadFromJsonAsync<BookingInfo>()
            : null;
        var location = postResponse?.Headers.Location?.ToString();
        
        var getResponse = location is not null
            ? await GetAsync(location, accessToken)
            : null;

        getResponse?.EnsureSuccessStatusCode();

        BookingInfo? located = getResponse is not null 
            ? await getResponse.Content.ReadFromJsonAsync<BookingInfo>()
            : null;
    
        // Then
        Assert.NotNull(postResponse);
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);
        Assert.NotNull(location);
        Assert.NotNull(created);
        Assert.NotNull(getResponse);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(eventId, created.EventId);
        Assert.NotNull(located);
        Assert.Equal(eventId, located.EventId);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_NotFound()
    {
        // Given
        Guid eventId = Guid.Empty;
        string accessToken = await LoginUser("user", "user");
    
        // When
        var postResponse = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken);
    
        // Then
        Assert.NotNull(postResponse);
        Assert.Equal(HttpStatusCode.NotFound, postResponse.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_PastEvent_BadRequest()
    {
        // Given
        Guid eventId = (await CreatePastEvent())!.Id;
        string accessToken = await LoginUser("user", "user");
    
        // When
        var postResponse = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken);
    
        // Then
        Assert.NotNull(postResponse);
        Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_Unauthorized()
    {
        // Given
        Guid eventId = (await CreateRandomEvent())!.Id;
    
        // When
        var postResponse = await PostAsync($"/events/{eventId}/book", (object)null!);
            
        // Then
        Assert.NotNull(postResponse);
        Assert.Equal(HttpStatusCode.Unauthorized, postResponse.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_NoAvailableSeats()
    {
        // Given
        Guid eventId = (await CreateRandomEvent())!.Id;
        string accessToken = await LoginUser("user", "user");
    
        // When
        var success = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken);
        var fail = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken);
            
        // Then
        Assert.NotNull(success);
        Assert.Equal(HttpStatusCode.Accepted, success.StatusCode);
        Assert.NotNull(fail);
        Assert.Equal(HttpStatusCode.Conflict, fail.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Post_LimitReached()
    {
        // Given
        EventInputData eventData = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(1))
            .HasTotalSeats(50)
            .Build();
        
        Guid eventId = (await CreateEvent(eventData))!.Id;

        string userName_1 = $"{nameof(Bookings_Post_LimitReached)}_1";
        string userName_2 = $"{nameof(Bookings_Post_LimitReached)}_2";;
        const string password = nameof(password);

        await CreateUser(userName_1, password);
        await CreateUser(userName_2, password);

        string token_1 = await LoginUser(userName_1, password);
        string token_2 = await LoginUser(userName_2, password);
        const int MaxBookingCount = 10;
    
        // When
        List<Task> successBookings = new(MaxBookingCount * 2);
        
        successBookings.AddRange(Enumerable.Range(0, MaxBookingCount)
            .Select(async(_)  =>
            {
                var response = await PostAsync($"/events/{eventId}/book", (object)null!, token_1);
                response.EnsureSuccessStatusCode();
            }));

        successBookings.AddRange(Enumerable.Range(0, MaxBookingCount)
            .Select(async(_) => 
            {
                var response = await PostAsync($"/events/{eventId}/book", (object)null!, token_2);
                response.EnsureSuccessStatusCode();
            }));

        await Task.WhenAll(successBookings);
        
        var failure_1 = await PostAsync($"/events/{eventId}/book", (object)null!, token_1);
        var failure_2 = await PostAsync($"/events/{eventId}/book", (object)null!, token_2);
            
        // Then
        Assert.True(token_2 != token_1);
        Assert.NotNull(failure_1);
        Assert.Equal(HttpStatusCode.Conflict, failure_1.StatusCode);
        Assert.NotNull(failure_2);
        Assert.Equal(HttpStatusCode.Conflict, failure_2.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Delete_Success()
    {
        // Given
        string userName_1 = $"{nameof(Bookings_Delete_Success)}_1";
        string userName_2 = $"{nameof(Bookings_Delete_Success)}_2";;
        const string password = nameof(password);

        await CreateUser(userName_1, password);
        await CreateUser(userName_2, password);

        EventInputData eventData = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(1))
            .HasTotalSeats(2)
            .Build();
        Guid eventId = (await CreateEvent(eventData))!.Id;

        // When
        string accessToken_1 = await LoginUser(userName_1, password);
        string accessToken_2 = await LoginUser(userName_2, password);

        var booking_1 = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken_1);
        var booking_2 = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken_2);

        booking_1.EnsureSuccessStatusCode();
        booking_2.EnsureSuccessStatusCode();

        BookingInfo? bookingInfo_1 = await booking_1.Content.ReadFromJsonAsync<BookingInfo>();
        BookingInfo? bookingInfo_2 = await booking_2.Content.ReadFromJsonAsync<BookingInfo>();

        var cancel_1 = bookingInfo_1 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_1.Id}", accessToken_1)
            : null;
        var cancel_2 = bookingInfo_2 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_2.Id}", accessToken_2)
            : null;
        
        cancel_1?.EnsureSuccessStatusCode();
        cancel_2?.EnsureSuccessStatusCode();

        // Then
        Assert.NotNull(cancel_1);
        Assert.Equal(HttpStatusCode.NoContent, cancel_1.StatusCode);
        Assert.NotNull(cancel_2);
        Assert.Equal(HttpStatusCode.NoContent, cancel_2.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Delete_Forbidden()
    {
        // Given
        string userName_1 = $"{nameof(Bookings_Delete_Forbidden)}_1";
        string userName_2 = $"{nameof(Bookings_Delete_Forbidden)}_2";;
        const string password = nameof(password);

        await CreateUser(userName_1, password);
        await CreateUser(userName_2, password);

        EventInputData eventData = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(1))
            .HasTotalSeats(2)
            .Build();
        Guid eventId = (await CreateEvent(eventData))!.Id;

        // When
        string accessToken_1 = await LoginUser(userName_1, password);
        string accessToken_2 = await LoginUser(userName_2, password);
        string adminToken = await LoginUser("admin", "admin");

        var booking_1 = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken_1);
        var booking_2 = await PostAsync($"/events/{eventId}/book", (object)null!, accessToken_2);

        booking_1.EnsureSuccessStatusCode();
        booking_2.EnsureSuccessStatusCode();

        BookingInfo? bookingInfo_1 = await booking_1.Content.ReadFromJsonAsync<BookingInfo>();
        BookingInfo? bookingInfo_2 = await booking_2.Content.ReadFromJsonAsync<BookingInfo>();

        var failure_1 = bookingInfo_1 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_1.Id}", accessToken_2)
            : null;
        var failure_2 = bookingInfo_2 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_2.Id}", accessToken_1)
            : null;
        
        var success_1 = bookingInfo_1 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_1.Id}", adminToken)
            : null;
        var success_2 = bookingInfo_2 is not null
            ? await DeleteAsync($"/bookings/{bookingInfo_2.Id}", adminToken)
            : null;

        success_1?.EnsureSuccessStatusCode();
        success_2?.EnsureSuccessStatusCode();

        // Then
        Assert.NotNull(failure_1);
        Assert.Equal(HttpStatusCode.Forbidden, failure_1.StatusCode);
        Assert.NotNull(failure_2);
        Assert.Equal(HttpStatusCode.Forbidden, failure_2.StatusCode);
        Assert.NotNull(success_1);
        Assert.Equal(HttpStatusCode.NoContent, success_1.StatusCode);
        Assert.NotNull(success_2);
        Assert.Equal(HttpStatusCode.NoContent, success_2.StatusCode);
    }
    
    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Delete_NotFound()
    {
        // Given
        string accessToken = await LoginUser("user", "user");

        // When
        var response = await DeleteAsync($"/bookings/{Guid.Empty}", accessToken);

        // Then
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Trait(Category, Category_Web)]
    [Fact]
    public async Task Bookings_Delete_Unauthorized()
    {
        // Given
        EventInputData eventData = new EventDataBuilder()
            .StartAt(DateTime.UtcNow.AddDays(1))
            .HasTotalSeats(2)
            .Build();
        Guid eventId = (await CreateEvent(eventData))!.Id;
        string bookingToken = await LoginUser("user", "user");
        string deleteToken = null!;

        // When
        var booking = await PostAsync($"/events/{eventId}/book", (object)null!, bookingToken);

        booking.EnsureSuccessStatusCode();

        BookingInfo? bookingInfo = await booking.Content.ReadFromJsonAsync<BookingInfo>();

        var cancel = bookingInfo is not null
            ? await DeleteAsync($"/bookings/{bookingInfo.Id}", deleteToken)
            : null;
        
        // Then
        Assert.NotNull(cancel);
        Assert.Equal(HttpStatusCode.Unauthorized, cancel.StatusCode);
    }
}