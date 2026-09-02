using EventManager.Models;

namespace EventManager.Application.Interfaces;

public interface IBookingRepository : IObjectRepository<Booking, Guid>
{
    
}