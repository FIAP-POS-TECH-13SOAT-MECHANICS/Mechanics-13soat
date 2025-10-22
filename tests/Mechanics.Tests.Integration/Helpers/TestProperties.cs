using Testcontainers.MsSql;

namespace Mechanics.Tests.Integration.Helpers;

[TestClass]
public static class TestProperties
{
    public static ApplicationFactory Factory { get; private set; } = null!;
    private static MsSqlContainer _container = null!;

    [AssemblyInitialize]
    public static async Task Setup(TestContext context)
    {
        _container = new TestDatabaseContainer().Container;
        await _container.StartAsync(context.CancellationTokenSource.Token);

        Environment.SetEnvironmentVariable("JwtOptions__AccessTokenLifetime", "60");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", _container.GetConnectionString());
        Factory = new ApplicationFactory();
    }

    [AssemblyCleanup]
    public static async Task Cleanup()
    {
        await Factory.DisposeAsync();
        await _container.DisposeAsync();
    }
}
