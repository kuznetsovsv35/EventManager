using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class FilterFixture : TestAppDbContext
{
    public IFilter<Event> Filter { get; } = new FilterService<Event>();
}