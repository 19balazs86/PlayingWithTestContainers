using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Infrastructure;

// Blog post: https://khalidabuhakmeh.com/fix-unable-to-resolve-dbcontextoptions-for-ef-core
// You can move the classes from the Data folder and this factory class into a separate class library: WebAPI.Data
// Command without the factory -> dotnet ef Migrations add "Initial" -s .\WebAPI\WebAPI.csproj -p .\WebAPI.Data\WebAPI.Data.csproj
// With this factory, go to the WebAPI.Data folder and simply run -> dotnet ef Migrations add "Initial"
// The WebAPI.Data project needs the following packages
// - Microsoft.EntityFrameworkCore.Relational (to resolve dependency conflict, due to HasFilter("\"IsDeleted\" = false"))
// - Microsoft.EntityFrameworkCore            (to resolve dependency conflict)
// - Microsoft.EntityFrameworkCore.Tools      (to run the EF migration command)
// - Npgsql.EntityFrameworkCore.PostgreSQL    (this factory calls: UseNpgsql(...))
public sealed class DesignTimeDbContextFactory //: IDesignTimeDbContextFactory<WebApiContext> // Commented out to make it NOT-functional
{
    private const string _connString = "Host=localhost;Port=5432;Username=postgres;Password=postgrespw;Database=postgres;";

    public WebApiContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WebApiContext>();

        optionsBuilder.UseNpgsql(_connString);

        return new WebApiContext(optionsBuilder.Options);
    }
}
