﻿using Alba;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;
using Respawn.Graph;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace WebAPI.IntegrationTests.Core;

public sealed class AlbaHostFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
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
        await _postgreSqlContainer.StartAsync();

        AlbaWebHost = await AlbaHost.For<Program>(configureWebHostBuilder);

        // DB migration is applied with a BackgroundService, therefore the Respawner needs to be crated later on.
        // Otherwise no tables and ResetDatabase throws exception.
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
            ["ConnectionStrings:PostgreSQL"] = _postgreSqlContainer.GetConnectionString()
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
        _dbConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());

        await _dbConnection.OpenAsync();

        return await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task DisposeAsync()
    {
        await AlbaWebHost.DisposeAsync();

        await _dbConnection.DisposeAsync();

        await _postgreSqlContainer.StopAsync();
    }
}
