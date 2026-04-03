using api.CZ.Features.Administrators.Services;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.Administrators.Factories;

namespace api.CZ.Features.Administrators;

public static class AdministratorExtensions
{
    public static IServiceCollection AddAdministratorServices(this IServiceCollection services)
    {
        services.AddScoped<IAdministratorRepository, AdministratorRepository>();
        services.AddScoped<IAdministratorService, AdministratorService>();
        services.AddScoped<IAdministratorFactory, AdministratorFactory>();

        return services;
    }
}
