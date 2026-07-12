using System.Collections.Concurrent;
using System.Threading;
using EventManager.Application.Interfaces;
using EventManager.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EventManager.Application.Services;

public class BookingQueue : IBookingQueue
{
    readonly ConcurrentQueue<Booking> _queue = new();
    readonly SemaphoreSlim _trigger = new(0, 1);
    
    public async Task<Booking> DequeueBooking(CancellationToken cancellation)
    {
        if (!_queue.IsEmpty)
            return _queue.Take(1).First();

        await _trigger.WaitAsync(cancellation);
        return _queue.Take(1).First();
    }

    public void EnqueueBooking(Booking booking)
    {
        _queue.Enqueue(booking);
        _trigger.Release();
    }
}