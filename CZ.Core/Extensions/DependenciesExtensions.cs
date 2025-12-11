using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace api.CZ.Core.Extensions;

public static class DependenciesExtensions
{
    public static void InjectDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.AddRepositories();
        builder.AddServices();
        builder.AddJwt();
        builder.AddSwagger();
        builder.AddEfCoreConfiguration();
    }

    private static void AddServices(this WebApplicationBuilder builder)
    {
        return;
    }

    private static void AddRepositories(this WebApplicationBuilder builder)
    {
        return;
    }

    private static void AddJwt(this WebApplicationBuilder builder)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                        throw new InvalidOperationException("JWT secret 'JWT_SECRET' not found.");

        var key = Encoding.ASCII.GetBytes(jwtSecret);

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // True in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false, 
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        builder.Services.AddAuthorization();
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "CESIZen", Version = "v1" });
        });
    }

    private static void AddEfCoreConfiguration(this WebApplicationBuilder builder)
    {
        return;
    }

    private static void AddCorsConfiguration(this WebApplicationBuilder builder)
    {
        var clientUrl = Environment.GetEnvironmentVariable("URL_CLIENT") ??
                           throw new InvalidOperationException("Client app URL 'URL_CLIENT' not found.");

        var backofficeUrl = Environment.GetEnvironmentVariable("URL_BACKOFFICE") ??
                               throw new InvalidOperationException("Backlog URL 'URL_BACKOFFICE' not found.");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowClientApp",
                policy =>
                {
                    policy.WithOrigins(clientUrl)
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            options.AddPolicy("AllowBackLog",
                policy =>
                {
                    policy.WithOrigins(backofficeUrl)
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }
}