using Common.Tests.Postgres;
using BookingService.Database;

namespace BookingService.IntegrationTests;

[CollectionDefinition(nameof(ContainersCollection))]
public class ContainersCollection() : ICollectionFixture<ContainerWrapper>;

public class ContainerWrapper() : ContainerWrapper<BookingsDbContext>, IAsyncLifetime;

public class DatabaseTestBase(ContainerWrapper container) : DatabaseTestBase<BookingsDbContext>(container),
    IClassFixture<ContainerWrapper>;
