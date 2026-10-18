using EventManager.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.WebTests;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }

    public DatabaseFixture(ApiTestFactory factory)
    {        
        Context = factory.Services.GetRequiredService<AppDbContext>();
        Context.Database.Migrate();
        SeedData();
    }

    private void SeedData()
    {
    }

    public void Dispose()
    {
    }
}
