using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Common.Tests.Postgres;

public abstract class DatabaseTestBase<TDbContext>(TestContainerWrapper<TDbContext> testContainer) 
    : IClassFixture<TestContainerWrapper<TDbContext>> 
    where TDbContext : DbContext
{
    protected TDbContext CreateDbContext() => testContainer.CreateDbContext();
    protected Task ResetDatabase() => testContainer.ResetDatabase();
}