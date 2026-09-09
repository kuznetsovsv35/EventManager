using EventManager.Data;

namespace EventManager.IntegrationTests;

public class UnitTest1 : DatabaseTestBase
{
    [Trait(Category, Category_Integration)]
    [Fact]
    public async Task Test1()
    {
        await using var context = CreateDbContext<AppDbContext>();        
        Assert.True(context.Database.CanConnect());
    }
}
