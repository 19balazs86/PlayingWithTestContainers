using Alba;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace WebAPI.IntegrationTests.Core;

public sealed class AlbaHostFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(PostgreSqlBuilder.PostgreSqlPort)) // Currently, it is not necessary, but good practice to apply WaitStrategy
        .WithImage("postgres:latest")
        .WithDatabase("database")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private DbConnection _dbConnection = default!;

    private Respawner? _respawner;

    private Lazy<Task<Respawner>> _lazyRespawner = default!;

    public IAlbaHost AlbaWebHost { get; set; } = default!;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Using EF, you can create an SQL migration script and execute it, instead of applying it through a BackgroundService
        // await _postgreSqlContainer.ExecScriptAsync(File.ReadAllText(@"....\WebAPI\migration_script.sql"));

        AlbaWebHost = await AlbaHost.For<Program>(configureWebHostBuilder);

        // DB migration is applied using a BackgroundService, therefore, the Respawner must be created afterward
        // Otherwise, ResetDatabase will throw an exception due to missing tables
        _lazyRespawner = new Lazy<Task<Respawner>>(createRespawner);
    }

    private void configureWebHostBuilder(IWebHostBuilder webHostBuilder)
    {
        webHostBuilder.ConfigureAppConfiguration(configureAppConfiguration);
    }

    private void configureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        var configurationOverridden = new Dictionary<string, string>
        {
            ["ConnectionStrings:PostgreSQL"] = _postgresContainer.GetConnectionString()
        };

        configurationBuilder.AddInMemoryCollection(configurationOverridden!);
    }

    public async Task ResetDatabaseAsync()
    {
        _respawner ??= await _lazyRespawner.Value;

        await _respawner.ResetAsync(_dbConnection);
    }

    private async Task<Respawner> createRespawner()
    {
        _dbConnection = new NpgsqlConnection(_postgresContainer.GetConnectionString());

        await _dbConnection.OpenAsync();

        return await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter        = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore   = ["__EFMigrationsHistory"]
        });
    }

    public async Task DisposeAsync()
    {
        await AlbaWebHost.DisposeAsync();

        await _dbConnection.DisposeAsync();

        await _postgresContainer.StopAsync();
    }
}
