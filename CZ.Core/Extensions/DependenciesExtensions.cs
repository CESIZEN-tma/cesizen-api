using System.Text;
using api.CZ.Core.Services;
using api.CZ.Core.Utils;
using api.CZ.Data.EFCore;
using api.CZ.Features.Authentifications.Services;
using api.CZ.Features.EmailConfirmationTokens.Factories;
using api.CZ.Features.EmailConfirmationTokens.Repositories;
using api.CZ.Features.EmailConfirmationTokens.Services;
using api.CZ.Features.HealthChecks.Services;
using api.CZ.Features.PasswordResetTokens.Factories;
using api.CZ.Features.PasswordResetTokens.Repositories;
using api.CZ.Features.PasswordResetTokens.Services;
using api.CZ.Features.Sessions.Factories;
using api.CZ.Features.Sessions.Repositories;
using api.CZ.Features.Sessions.Services;
using api.CZ.Features.Users.Factories;
using api.CZ.Features.Users.Repositories;
using api.CZ.Features.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Simply.Auth.Argon2.Configuration;
using Simply.Auth.Argon2.Services;
using Simply.Auth.AspNetCore.Extensions;
using Simply.Auth.Core.Abstractions;

namespace api.CZ.Core.Extensions;

public static class DependenciesExtensions
{
    public static void InjectDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.AddSimply(65536,3,4,"CesiZen-api","CesiZen-front");
        builder.AddRepositories();
        builder.AddServices();
        builder.AddFactories();
        builder.AddSwagger();
        builder.AddEfCoreConfiguration();
    }

    private static void AddFactories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserFactory, UserFactory>();
        builder.Services.AddScoped<IEmailConfirmationTokenFactory, EmailConfirmationTokenFactory>();
        builder.Services.AddScoped<IPasswordResetTokenFactory, PasswordResetTokenFactory>();
        builder.Services.AddScoped<ISessionFactory, SessionFactory>();
    }

    private static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
        builder.Services.AddScoped<IAuthentificationService, AuthentificationService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IEmailConfirmationTokenService, EmailConfirmationTokenService>();
        builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        builder.Services.AddScoped<ISessionService, SessionService>();
    }

    private static void AddRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
        builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    }
    
    private static void AddSimply(this WebApplicationBuilder builder, int memorySize, int iterations,
        int parallelismDegree, string issuer, string audience)
    {
        string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                           ?? throw new Exception("JWT_SECRET not found");

        
        builder.Services.AddSimplyAuth(
            argon2 =>
            {
                argon2.MemorySize = memorySize;
                argon2.Iterations = iterations;
                argon2.DegreeOfParallelism = parallelismDegree;
            },
            jwt =>
            {
                jwt.SecretKey = jwtSecret;
                jwt.Issuer = issuer;
                jwt.Audience = audience;
            });
        
        var descriptor = builder.Services.FirstOrDefault(d => 
            d.ServiceType == typeof(ISimplyPasswordHasher));
    
        if (descriptor != null)
            builder.Services.Remove(descriptor);
        
        builder.Services.AddSingleton<ISimplyPasswordHasher>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SimplyArgon2Options>>();
            return new SimplyArgon2Hasher(options);
        });
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
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DATABASE_CONNECTION_STRING' not found.");

        builder.Services.AddDbContext<CesiZenDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.CommandTimeout(30)));
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