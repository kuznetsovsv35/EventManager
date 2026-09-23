using System.Threading.Channels;
using EventManager.Application.Interfaces;

namespace EventManager.Infrastructure.Services;

public class BookingServiceNotifier : IBookingServiceNotifier
{
    readonly Channel<Guid> _channel = Channel.CreateBounded<Guid>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleWriter = false,
        SingleReader = true
    });

    public Task BookingCreatedAsync(Guid bookingId, CancellationToken cancellation)
        => _channel.Writer.WriteAsync(bookingId, cancellation).AsTask();

    public async Task<IEnumerable<Guid>?> WaitBookingCreationAsync(CancellationToken cancellation)
    {
        if (await _channel.Reader.WaitToReadAsync(cancellation))
        {
            List<Guid> items = new(_channel.Reader.Count);

            while (_channel.Reader.TryRead(out var item))
            {
                items.Add(item);
                cancellation.ThrowIfCancellationRequested();
            }

            return items;
        }
        return null;
    }
}