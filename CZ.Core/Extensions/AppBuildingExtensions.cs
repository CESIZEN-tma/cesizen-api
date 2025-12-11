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
        
        app.AddAccessMiddlewares();
        
        app.MapControllers();
        
        app.MapGet("/", () =>
        {
            string print = "hey";
            return new
            {
                print
            };
        });
        
        app.Run();
    }

    private static void AddAccessMiddlewares(this WebApplication app)
    {
        app.UseAuthorization();
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