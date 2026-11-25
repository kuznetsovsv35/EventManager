using Common.Tests.Postgres;
using UsersService.Database;

namespace UsersService.IntegrationTests;

[CollectionDefinition(nameof(ContainersCollection))]
public class ContainersCollection() : ICollectionFixture<ContainerWrapper>;

public class ContainerWrapper() : ContainerWrapper<UsersDbContext>, IAsyncLifetime;

public class DatabaseTestBase(ContainerWrapper container) : DatabaseTestBase<UsersDbContext>(container),
    IClassFixture<ContainerWrapper>;
