using Common.Tests.Postgres;
using UsersService.Database;

namespace UsersService.IntegrationTests;

public class ContainerWrapper : TestContainerWrapper<UsersDbContext>, IAsyncLifetime { }
