using Common.Tests.Postgres;
using EventsService.Database;

namespace EventsService.IntegrationTests;

[CollectionDefinition(nameof(ContainersCollection))]
public class ContainersCollection() : ICollectionFixture<ContainerWrapper>;

public class ContainerWrapper() : ContainerWrapper<EventsDbContext>, IAsyncLifetime;

public class DatabaseTestBase(ContainerWrapper container) : DatabaseTestBase<EventsDbContext>(container),
    IClassFixture<ContainerWrapper>;
