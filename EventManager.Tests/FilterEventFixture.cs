using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class FilterEventFixture : TestAppDbContext
{
    public IFilter<Event> FilterService { get; } = new FilterService<Event>();
}