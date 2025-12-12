using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.Administrators.Repositories;

public class AdministratorRepository : IAdministratorRepository
{
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
