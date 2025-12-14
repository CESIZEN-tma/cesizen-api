using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;

namespace api.CZ.Features.Administrators.Services;

public class AdministratorService : IAdministratorService
{
    private readonly IAdministratorRepository _repository;

    public AdministratorService(IAdministratorRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Administrator>> GetAllAdministratorsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Administrator?> GetAdministratorByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<Administrator> CreateAdministratorAsync(Administrator administrator)
    {
        throw new NotImplementedException();
    }

    public async Task<Administrator?> UpdateAdministratorAsync(int id, Administrator administrator)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAdministratorAsync(int id)
    {
        throw new NotImplementedException();
    }
}
