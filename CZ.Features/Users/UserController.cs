using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Users.Services;

namespace api.CZ.Features.Users;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }
}
