using System.Text;
using api.CZ.Core.Services;
using api.CZ.Core.Utils;
using api.CZ.Data.EFCore;
using api.CZ.Features.AdminEmailConfirmationTokens.Factories;
using api.CZ.Features.AdminEmailConfirmationTokens.Repositories;
using api.CZ.Features.AdminEmailConfirmationTokens.Services;
using api.CZ.Features.AdminLogs.Factories;
using api.CZ.Features.AdminLogs.Repositories;
using api.CZ.Features.AdminLogs.Services;
using api.CZ.Features.AdminPasswordResetTokens.Factories;
using api.CZ.Features.AdminPasswordResetTokens.Repositories;
using api.CZ.Features.AdminPasswordResetTokens.Services;
using api.CZ.Features.AdminSessions.Factories;
using api.CZ.Features.AdminSessions.Repositories;
using api.CZ.Features.AdminSessions.Services;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Administrators.Services;
using api.CZ.Features.Authentifications.Services;
using api.CZ.Features.Bookmarks.Factories;
using api.CZ.Features.Bookmarks.Repositories;
using api.CZ.Features.Bookmarks.Services;
using api.CZ.Features.Configurations.Repositories;
using api.CZ.Features.Configurations.Services;
using api.CZ.Features.InformationPages.Repositories;
using api.CZ.Features.InformationPages.Services;
using api.CZ.Features.InformationTags.Repositories;
using api.CZ.Features.InformationTags.Services;
using api.CZ.Features.NavigationMenus.Repositories;
using api.CZ.Features.NavigationMenus.Services;
using api.CZ.Features.PasswordsInfos.Repositories;
using api.CZ.Features.PasswordsInfos.Services;
using api.CZ.Features.PasswordHistories.Repositories;
using api.CZ.Features.PasswordHistories.Services;
using api.CZ.Features.UserSavedConfigurations.Repositories;
using api.CZ.Features.UserSavedConfigurations.Services;
using api.CZ.Features.Documentation.Services;
using api.CZ.Features.EmailConfirmationTokens.Factories;
using api.CZ.Features.EmailConfirmationTokens.Repositories;
using api.CZ.Features.EmailConfirmationTokens.Services;
using api.CZ.Features.HealthChecks.Services;
using api.CZ.Features.PasswordResetTokens.Factories;
using api.CZ.Features.PasswordResetTokens.Repositories;
using api.CZ.Features.PasswordResetTokens.Services;
using api.CZ.Features.Quizzes.Factories;
using api.CZ.Features.Quizzes.Repositories;
using api.CZ.Features.Quizzes.Services;
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
        builder.AddCorsConfiguration();
        builder.AddEfCoreConfiguration();
    }

    private static void AddFactories(this WebApplicationBuilder builder)
    {
        // User factories
        builder.Services.AddScoped<IUserFactory, UserFactory>();
        builder.Services.AddScoped<IEmailConfirmationTokenFactory, EmailConfirmationTokenFactory>();
        builder.Services.AddScoped<IPasswordResetTokenFactory, PasswordResetTokenFactory>();
        builder.Services.AddScoped<ISessionFactory, SessionFactory>();

        // Admin factories
        builder.Services.AddScoped<IAdministratorFactory, AdministratorFactory>();
        builder.Services.AddScoped<IAdminEmailConfirmationTokenFactory, AdminEmailConfirmationTokenFactory>();
        builder.Services.AddScoped<IAdminPasswordResetTokenFactory, AdminPasswordResetTokenFactory>();
        builder.Services.AddScoped<IAdminSessionFactory, AdminSessionFactory>();
        builder.Services.AddScoped<IAdminLogFactory, AdminLogFactory>();

        // Bookmark factories
        builder.Services.AddScoped<IBookmarkFactory, BookmarkFactory>();

        // Quiz factories
        builder.Services.AddScoped<IQuizzFactory, QuizzFactory>();
    }

    private static void AddServices(this WebApplicationBuilder builder)
    {
        // Common services
        builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IDocumentationService, DocumentationService>();

        // User services
        builder.Services.AddScoped<IAuthentificationService, AuthentificationService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IEmailConfirmationTokenService, EmailConfirmationTokenService>();
        builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>();
        builder.Services.AddScoped<ISessionService, SessionService>();

        // Admin services
        builder.Services.AddScoped<IAdminAuthentificationService, AdminAuthentificationService>();
        builder.Services.AddScoped<IAdministratorService, AdministratorService>();
        builder.Services.AddScoped<IAdminEmailConfirmationTokenService, AdminEmailConfirmationTokenService>();
        builder.Services.AddScoped<IAdminPasswordResetTokenService, AdminPasswordResetTokenService>();
        builder.Services.AddScoped<IAdminSessionService, AdminSessionService>();
        builder.Services.AddScoped<IAdminLogService, AdminLogService>();
        builder.Services.AddScoped<IAdminActionLogger, AdminActionLogger>();

        // Bookmark services
        builder.Services.AddScoped<IBookmarkService, BookmarkService>();

        // Quiz services
        builder.Services.AddScoped<IQuizzService, QuizzService>();

        // Configuration services
        builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

        // InformationPage services
        builder.Services.AddScoped<IInformationPageService, InformationPageService>();

        // InformationTag services
        builder.Services.AddScoped<IInformationTagService, InformationTagService>();

        // NavigationMenu services
        builder.Services.AddScoped<INavigationMenuService, NavigationMenuService>();

        // PasswordsInfo services
        builder.Services.AddScoped<IPasswordsInfoService, PasswordsInfoService>();

        // PasswordHistory services
        builder.Services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
        builder.Services.AddScoped<IPasswordHistoryManager, PasswordHistoryManager>();

        // UserSavedConfiguration services
        builder.Services.AddScoped<IUserSavedConfigurationService, UserSavedConfigurationService>();
    }

    private static void AddRepositories(this WebApplicationBuilder builder)
    {
        // User repositories
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEmailConfirmationTokenRepository, EmailConfirmationTokenRepository>();
        builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();

        // Admin repositories
        builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        builder.Services.AddScoped<IAdminEmailConfirmationTokenRepository, AdminEmailConfirmationTokenRepository>();
        builder.Services.AddScoped<IAdminPasswordResetTokenRepository, AdminPasswordResetTokenRepository>();
        builder.Services.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
        builder.Services.AddScoped<IAdminLogRepository, AdminLogRepository>();

        // Bookmark repositories
        builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();

        // Quiz repositories
        builder.Services.AddScoped<IQuizzRepository, QuizzRepository>();
        builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
        builder.Services.AddScoped<IResponsesOptionRepository, ResponsesOptionRepository>();

        // Configuration repositories
        builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();

        // InformationPage repositories
        builder.Services.AddScoped<IInformationPageRepository, InformationPageRepository>();

        // InformationTag repositories
        builder.Services.AddScoped<IInformationTagRepository, InformationTagRepository>();

        // NavigationMenu repositories
        builder.Services.AddScoped<INavigationMenuRepository, NavigationMenuRepository>();

        // PasswordsInfo repositories
        builder.Services.AddScoped<IPasswordsInfoRepository, PasswordsInfoRepository>();

        // PasswordHistory repositories
        builder.Services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();

        // UserSavedConfiguration repositories
        builder.Services.AddScoped<IUserSavedConfigurationRepository, UserSavedConfigurationRepository>();
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
        var clientUrl = Environment.GetEnvironmentVariable("URL_FRONT") ??
                        throw new InvalidOperationException("Client app URL 'URL_FRONT' not found.");

        var backofficeUrl = Environment.GetEnvironmentVariable("URL_BACKOFFICE") ??
                            throw new InvalidOperationException("Backlog URL 'URL_BACKOFFICE' not found.");

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy.WithOrigins(clientUrl, backofficeUrl)
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }
}