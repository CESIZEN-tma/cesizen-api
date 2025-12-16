using api.CZ.Features.Users.Models;
using api.CZ.Features.Users.Repositories;

namespace api.CZ.Features.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
}
