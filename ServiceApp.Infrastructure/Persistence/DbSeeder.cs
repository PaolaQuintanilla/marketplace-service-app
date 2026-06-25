using Microsoft.EntityFrameworkCore;
using ServiceApp.Application.Interfaces;
using ServiceApp.Domain.Entities;
using ServiceApp.Domain.Enums;

namespace ServiceApp.Infrastructure.Persistence;

/// <summary>
/// Seeds a baseline admin account and a few service categories so the API is usable on a fresh database.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (!await context.Users.AnyAsync(u => u.Role == UserRole.Admin, cancellationToken))
        {
            context.Users.Add(new User
            {
                Name = "Administrator",
                Email = "admin@serviceapp.local",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = UserRole.Admin
            });
        }

        if (!await context.Services.AnyAsync(cancellationToken))
        {
            context.Services.AddRange(
                new Service { Name = "Plumbing", Category = "Home", Description = "Pipes, leaks and fittings." },
                new Service { Name = "Cleaning", Category = "Home", Description = "Residential and office cleaning." },
                new Service { Name = "Electrical", Category = "Home", Description = "Wiring, outlets and fixtures." },
                new Service { Name = "Gardening", Category = "Outdoor", Description = "Lawn care and landscaping." });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
