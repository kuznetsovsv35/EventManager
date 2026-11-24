using Common.Tests.Postgres;
using UsersService.Database;

namespace UsersService.IntegrationTests;

[CollectionDefinition(nameof(TestContainersCollection))]
public class TestContainersCollection : ICollectionFixture<TestContainerWrapper<UsersDbContext>> { }