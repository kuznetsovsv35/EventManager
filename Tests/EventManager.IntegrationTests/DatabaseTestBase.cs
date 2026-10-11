using EventManager.Database;
using EventManager.IntegrationTests.Extensions;

namespace EventManager.IntegrationTests;

public abstract class DatabaseTestBase(TestContainerWrapper<AppDbContext> testContainer) : IClassFixture<TestContainerWrapper<AppDbContext>>
{
    protected const string Category = "Category";
    protected const string Category_Database = "Database";
    protected const string Category_Repositories = "Repositories";

    protected AppDbContext CreateDbContext() => testContainer.CreateDbContext();
    protected Task ResetDatabase() => testContainer.ResetDatabase();

    protected async Task<Guid> GetAdminUser()
    {
        await using var context = CreateDbContext();
        return await context.EnsureCreatedUser(Domain.ValueObjects.UserRole.Admin);
    }
}