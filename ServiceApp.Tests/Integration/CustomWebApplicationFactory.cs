using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using ServiceApp.Infrastructure.Persistence;

namespace ServiceApp.Tests.Integration;

/// <summary>
/// Boots the real API in-process (TestServer) but swaps SQL Server for an EF in-memory
/// database, so the full HTTP pipeline — routing, model binding, auth, middleware, mapping —
/// is exercised without needing a real database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // A unique store name per factory instance keeps test classes isolated from each other.
    private readonly string _databaseName = "ServiceAppTests-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // ConfigureTestServices runs after the app's own registrations, so these overrides win.
        builder.ConfigureTestServices(services =>
        {
            // Strip every registration tied to the SQL Server AppDbContext. In EF Core 9+ the
            // provider (UseSqlServer) is an additive IDbContextOptionsConfiguration<AppDbContext>,
            // so removing only DbContextOptions<AppDbContext> would leave two providers registered.
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            // Replace it with an in-memory provider for the same context.
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
