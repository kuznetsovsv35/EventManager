using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Models;

namespace EventManager.Tests;

public class PaginatorFixture() : TestAppDbContext(nameof(PaginatorFixture))
{
    public IPaginator<Event> Paginator { get; } = new PaginateService<Event>();
}