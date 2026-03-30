using api.CZ.Core.Middlewares;
using api.CZ.Data.EFCore;
using Microsoft.EntityFrameworkCore;
using Simply.Auth.Core.Abstractions;

namespace api.CZ.Core.Extensions;

public static class AppBuildingExtensions
{
    public static async Task BuildSolution(this WebApplicationBuilder builder)
    {
        builder.InjectDependencies();
        var app = builder.Build();


        // CORS
        app.AddCorsRules();

        app.UseHttpsRedirection();

        app.AddMiddlewares();

        app.AddOpenApiMapping();

        app.MapControllers().WithGroupName("api");


        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CesiZenDbContext>();
            await MarkInitialMigrationIfNeededAsync(db);
            await db.Database.MigrateAsync();
            var authService = scope.ServiceProvider.GetRequiredService<ISimplyAuthService>();
            await Seeding.DatabaseSeeder.SeedAsync(db, authService);
        }

        app.Run();
    }

    // Migrations whose schema is already covered by init.sql.
    // These are pre-registered so MigrateAsync skips them on a fresh init.sql database.
    private static readonly string[] MigrationsPreAppliedByInitSql =
    [
        "20260111140528_RefactorPasswordResetTokenFK",
    ];

    // If init.sql bootstrapped the DB, pre-register the covered migrations
    // so MigrateAsync only applies truly pending ones (migrations 2 and 3).
    private static async Task MarkInitialMigrationIfNeededAsync(CesiZenDbContext db)
    {
        var appliedMigrations = (await db.Database.GetAppliedMigrationsAsync()).ToHashSet();
        bool alreadyHandled = MigrationsPreAppliedByInitSql.All(m => appliedMigrations.Contains(m));
        if (alreadyHandled) return;

        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync();

        using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'administrators' AND table_schema = 'public'";
            var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
            if (count == 0)
            {
                await conn.CloseAsync();
                return;
            }
        }

        using (var createCmd = conn.CreateCommand())
        {
            createCmd.CommandText = @"CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId""))";
            await createCmd.ExecuteNonQueryAsync();
        }

        foreach (var migration in MigrationsPreAppliedByInitSql)
        {
            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = @"INSERT INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                                      VALUES (@id, @version) ON CONFLICT DO NOTHING";
            var pId = insertCmd.CreateParameter(); pId.ParameterName = "@id"; pId.Value = migration; insertCmd.Parameters.Add(pId);
            var pVer = insertCmd.CreateParameter(); pVer.ParameterName = "@version"; pVer.Value = "10.0.0"; insertCmd.Parameters.Add(pVer);
            await insertCmd.ExecuteNonQueryAsync();
        }

        await conn.CloseAsync();
    }

    private static void AddMiddlewares(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseMiddleware<ExceptionMiddleware>();
    }

    private static void AddCorsRules(this WebApplication app)
    {
        app.UseCors("AllowAll");
    }

    private static void AddOpenApiMapping(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}