using EventManager.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

public abstract class DatabaseTestBase(TestContainerWrapper<AppDbContext> testContainer) : IClassFixture<TestContainerWrapper<AppDbContext>>
{
    protected const string Category = "Category";
    protected const string Category_Database = "Database";
    protected const string Category_Repositories = "Repositories";

    protected AppDbContext CreateDbContext() => testContainer.CreateDbContext();
    protected Task ResetDatabase() => testContainer.ResetDatabase();
}