using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Migrations;

public sealed class MigrationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MigrationBackgroundService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        WebApiContext dbContext = scope.ServiceProvider.GetRequiredService<WebApiContext>();

        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}
