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
        await SeedAdministratorsAsync(context, authService);
        await SeedUsersAsync(context, authService);
    }

    private static async Task SeedAdministratorsAsync(CesiZenDbContext context, ISimplyAuthService authService)
    {
        const string adminEmail = "admin@cesizen.fr";
        if (await context.Administrators.AnyAsync(a => a.Email == adminEmail && a.DeletionTime == null)) return;

        context.Administrators.Add(new Administrator
        {
            Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
            Email = adminEmail,
            PasswordHash = authService.HashPassword("Admin1234!"),
            FirstName = "Admin",
            LastName = "CesiZen",
            MemberSince = DateTime.UtcNow,
            AccountActivated = true,
            FailedLoginAttempts = 0,
            CreationTime = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(CesiZenDbContext context, ISimplyAuthService authService)
    {
        var users = new[]
        {
            ("user@cesizen.fr",  "User1234!", "Marie",   "Dupont"),
            ("demo@cesizen.fr",  "Demo1234!", "Thomas",  "Martin"),
            ("test@cesizen.fr",  "Test1234!", "Léa",     "Bernard"),
        };

        foreach (var (email, pwd, first, last) in users)
        {
            if (await context.Users.AnyAsync(u => u.Email == email && u.DeletionTime == null)) continue;
            context.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = authService.HashPassword(pwd),
                FirstName = first,
                LastName = last,
                MemberSince = DateTime.UtcNow,
                AccountActivated = true,
                Active = true,
                FailedLoginAttempts = 0,
                CreationTime = DateTime.UtcNow,
            });
        }
        await context.SaveChangesAsync();
    }
}
