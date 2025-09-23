using Testcontainers.MsSql;

namespace Mechanics.Tests.Integration.Helpers;

public class TestDatabaseContainer : IAsyncDisposable
{
    public MsSqlContainer Container { get; } = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("b0I6h9G%1zJo")
        .WithCleanUp(true)
        .Build();

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await Container.DisposeAsync();
    }
}
