using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Users.Models;
using api.CZ.Data.EFCore;
using Microsoft.EntityFrameworkCore;
using Simply.Auth.Core.Abstractions;

namespace api.CZ.Core.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CesiZenDbContext context, ISimplyAuthService authService)
    {
        await SeedAdministratorAsync(context, authService);
        await SeedUserAsync(context, authService);
    }

    private static async Task SeedAdministratorAsync(CesiZenDbContext context, ISimplyAuthService authService)
    {
        const string adminEmail = "admin@cesizen.fr";

        bool exists = await context.Administrators
            .AnyAsync(a => a.Email == adminEmail && a.DeletionTime == null);

        if (exists) return;

        var admin = new Administrator
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = authService.HashPassword("Admin1234!"),
            FirstName = "Admin",
            LastName = "CesiZen",
            MemberSince = DateTime.UtcNow,
            AccountActivated = true,
            FailedLoginAttempts = 0,
            CreationTime = DateTime.UtcNow,
        };

        context.Administrators.Add(admin);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUserAsync(CesiZenDbContext context, ISimplyAuthService authService)
    {
        const string userEmail = "user@cesizen.fr";

        bool exists = await context.Users
            .AnyAsync(u => u.Email == userEmail && u.DeletionTime == null);

        if (exists) return;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = userEmail,
            PasswordHash = authService.HashPassword("User1234!"),
            FirstName = "User",
            LastName = "CesiZen",
            MemberSince = DateTime.UtcNow,
            AccountActivated = true,
            Active = true,
            FailedLoginAttempts = 0,
            CreationTime = DateTime.UtcNow,
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
}