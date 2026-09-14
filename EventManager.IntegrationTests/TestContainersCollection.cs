using EventManager.Data;

namespace EventManager.IntegrationTests;

[CollectionDefinition(nameof(TestContainersCollection))]
public class TestContainersCollection : ICollectionFixture<TestContainerWrapper<AppDbContext>> { }