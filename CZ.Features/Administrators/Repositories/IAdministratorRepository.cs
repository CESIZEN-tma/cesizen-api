using api.CZ.Features.Administrators.Models;

namespace api.CZ.Features.Administrators.Repositories;

public interface IAdministratorRepository
{
    Task<IEnumerable<Administrator>> GetAllAdministratorsAsync();
    Task<Administrator?> GetAdministratorByIdAsync(int id);
    Task<Administrator> CreateAdministratorAsync(Administrator administrator);
    Task<Administrator?> UpdateAdministratorAsync(int id, Administrator administrator);
    Task<bool> DeleteAdministratorAsync(int id);
}
