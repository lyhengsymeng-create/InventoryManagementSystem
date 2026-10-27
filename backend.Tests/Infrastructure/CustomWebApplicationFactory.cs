
using System;
using System.Threading;
using System.Threading.Tasks;
using backend.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace backend.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            var connectionString =
                Environment.GetEnvironmentVariable(
                    "INVENTORY_TEST_CONNECTION_STRING");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Missing INVENTORY_TEST_CONNECTION_STRING environment variable.");
            }

            services.AddDbContext<AppDbContext>(
                options => options.UseNpgsql(connectionString));

            services.AddHostedService<ApplyTestDatabaseMigrations>();
        });
    }
}

internal sealed class ApplyTestDatabaseMigrations : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ApplyTestDatabaseMigrations(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Safety check: only migrate the dedicated integration-test database.
        var databaseName = db.Database.GetDbConnection().Database;

        if (!string.Equals(
                databaseName,
                "inventory_management_integration_test",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Refusing to migrate: the configured database is not the dedicated test database.");
        }

        await db.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
