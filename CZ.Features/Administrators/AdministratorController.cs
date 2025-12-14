using Microsoft.AspNetCore.Mvc;
using api.CZ.Features.Administrators.Services;

namespace api.CZ.Features.Administrators;

[ApiController]
[Route("api/[controller]")]
public class AdministratorController : ControllerBase
{
    private readonly IAdministratorService _service;
    private readonly ILogger<AdministratorController> _logger;

    public AdministratorController(IAdministratorService service, ILogger<AdministratorController> logger)
    {
        _service = service;
        _logger = logger;
    }
}
