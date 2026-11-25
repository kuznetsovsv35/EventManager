using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Common.Tests.Postgres;

public abstract class DatabaseTestBase<TDbContext>(ContainerWrapper<TDbContext> testContainer) where TDbContext : DbContext
{
    protected TDbContext CreateDbContext() => testContainer.CreateDbContext();
    protected Task ResetDatabase() => testContainer.ResetDatabase();

    protected async Task ExecuteTestMigrations(Action<ICollection<string>, ICollection<string>> assert)
    {
        // Given
        await ResetDatabase();
        await using var context = CreateDbContext();
        const string migrationId = $@"""{nameof(EFMigration.MigrationId)}""";
        const string productVersion = $@"""{nameof(EFMigration.ProductVersion)}""";
        const string tableName = $@"""__EFMigrationsHistory""";

        // When
        var expectedMigrations = typeof(TDbContext).Assembly
            .GetTypes()
            .Where(t => typeof(Migration).IsAssignableFrom(t))
            .Select(t => t.Name)
            .Order()
            .ToList();

        var query = context.Database
            .SqlQueryRaw<EFMigration>($@"
                SELECT {migrationId}, {productVersion} 
                FROM {tableName}
                ORDER BY {migrationId}
                ");

        var actualMigrations = await query
            .AsAsyncEnumerable()
            .Select(m => m.MigrationId.Split('_').Last())
            .Order()
            .ToListAsync();

        // Then
        assert(expectedMigrations, actualMigrations);
    }

    protected async Task ExecuteTestConnection(Action<bool> assert)
    {
        // Given
        await ResetDatabase();

        // When
        await using var context = CreateDbContext();

        // Then
        assert(context.Database.CanConnect());
    }
}