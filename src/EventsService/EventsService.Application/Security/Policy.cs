namespace EventsService.Application.Security;

public static class Policy
{
    public const string CreateEvent = nameof(CreateEvent);
    public const string DeleteEvent = nameof(DeleteEvent);
    public const string GetEvents = nameof(GetEvents);
    public const string ModifyEvent = nameof(ModifyEvent);
}
