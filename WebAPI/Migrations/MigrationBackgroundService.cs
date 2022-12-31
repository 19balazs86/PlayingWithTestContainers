using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Migrations
{
    public sealed class MigrationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public MigrationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();

            WebApiContext dbContext = scope.ServiceProvider.GetRequiredService<WebApiContext>();

            await dbContext.Database.MigrateAsync(stoppingToken);
        }
    }
}
