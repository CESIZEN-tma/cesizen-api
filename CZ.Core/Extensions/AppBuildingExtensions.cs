using api.CZ.Core.Middlewares;

namespace api.CZ.Core.Extensions;

public static class AppBuildingExtensions
{
    public static void BuildSolution(this WebApplicationBuilder builder)
    {
        builder.InjectDependencies();
        var app = builder.Build();
        
        
        
        // CORS
        app.AddCorsRules();

        app.UseHttpsRedirection();
        
        app.AddMiddlewares();
        
        app.AddOpenApiMapping();
        
        app.MapControllers().WithGroupName("api");
        
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
        app.UseCors("AllowClientApp");
        app.UseCors("AllowBacklog");
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