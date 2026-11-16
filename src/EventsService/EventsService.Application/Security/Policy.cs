namespace EventsService.Application.Security;

public static class Policy
{
    public const string CreateEvent = $"{nameof(EventsService)}.{nameof(CreateEvent)}";
    public const string DeleteEvent = $"{nameof(EventsService)}.{nameof(DeleteEvent)}";
    public const string GetEvents = $"{nameof(EventsService)}.{nameof(GetEvents)}";
    public const string ModifyEvent = $"{nameof(EventsService)}.{nameof(ModifyEvent)}";
}
