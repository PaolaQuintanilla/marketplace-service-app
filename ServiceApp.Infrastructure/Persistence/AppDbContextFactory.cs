using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceApp.Infrastructure.Persistence;

/// <summary>
/// Lets the EF Core CLI (`dotnet ef migrations ...`) build the context at design time
/// without starting the API host. The connection string here is only used by tooling.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SERVICEAPP_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ServiceAppDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
