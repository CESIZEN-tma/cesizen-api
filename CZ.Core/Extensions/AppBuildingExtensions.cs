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
            await db.Database.MigrateAsync();
            var authService = scope.ServiceProvider.GetRequiredService<ISimplyAuthService>();
            await Seeding.DatabaseSeeder.SeedAsync(db, authService);
        }

        app.Run();
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