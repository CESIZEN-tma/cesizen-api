using api.CZ.Features.Administrators.DTOs;
using api.CZ.Features.Administrators.Extensions;
using api.CZ.Features.Administrators.Factories;
using api.CZ.Features.Administrators.Models;
using api.CZ.Features.Administrators.Repositories;
using api.CZ.Features.AdminLogs.Services;
using Simply.Auth.Core.Abstractions;

namespace api.CZ.Features.Administrators.Services;

public class AdministratorService : IAdministratorService
{
    private readonly IAdministratorRepository _repository;
    private readonly IAdministratorFactory _factory;
    private readonly ISimplyAuthService _authService;
    private readonly IAdminActionLogger _actionLogger;

    public AdministratorService(
        IAdministratorRepository repository,
        IAdministratorFactory factory,
        ISimplyAuthService authService,
        IAdminActionLogger actionLogger)
    {
        _repository = repository;
        _factory = factory;
        _authService = authService;
        _actionLogger = actionLogger;
    }

    public async Task<IEnumerable<GetAdministratorDto>> GetAllAsync()
    {
        var admins = await _repository.ListAsync(a => a.DeletionTime == null);
        return admins.Select(a => a.ToDto());
    }

    public async Task<GetAdministratorDto?> GetByIdAsync(Guid id)
    {
        var admin = await _repository.FindAsync(id);

        if (admin == null || admin.DeletionTime != null)
            return null;

        return admin.ToDto();
    }

    public async Task<GetAdministratorDto?> CreateAsync(CreateAdministratorDto dto, Guid creatorAdminId)
    {
        // Check if email already exists
        if (await _repository.AnyAsync(a => a.Email == dto.Email))
            return null;

        var passwordHash = _authService.HashPassword(dto.Password);
        var admin = _factory.Create(dto.Email, dto.FirstName, dto.LastName, passwordHash);

        await _repository.AddAsync(admin);

        await _actionLogger.LogCreateAsync(creatorAdminId, "Administrator", admin.Id,
            $"Created administrator: {admin.FirstName} {admin.LastName} <{admin.Email}>");

        return admin.ToDto();
    }

    public async Task<GetAdministratorDto?> UpdateAsync(Guid id, UpdateAdministratorDto dto, Guid adminId)
    {
        var admin = await _repository.FindAsync(id);

        if (admin == null || admin.DeletionTime != null)
            return null;

        var changes = new List<string>();
        if (admin.FirstName != dto.FirstName) changes.Add($"FirstName: '{admin.FirstName}' → '{dto.FirstName}'");
        if (admin.LastName != dto.LastName) changes.Add($"LastName: '{admin.LastName}' → '{dto.LastName}'");
        if (admin.ThumbnailUrl != dto.ThumbnailUrl) changes.Add("ThumbnailUrl updated");
        var changesDescription = changes.Count > 0 ? string.Join(", ", changes) : "no changes";

        admin.FirstName = dto.FirstName;
        admin.LastName = dto.LastName;
        admin.ThumbnailUrl = dto.ThumbnailUrl;
        admin.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(admin);

        await _actionLogger.LogUpdateAsync(adminId, "Administrator", id,
            $"Updated administrator {admin.Email}: {changesDescription}");

        return admin.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, Guid adminId)
    {
        if (id == adminId)
            throw new InvalidOperationException("You cannot delete your own administrator account");

        var admin = await _repository.FindAsync(id);

        if (admin == null || admin.DeletionTime != null)
            return false;

        var adminEmail = admin.Email;

        admin.DeletionTime = DateTime.UtcNow;
        admin.UpdateTime = DateTime.UtcNow;

        await _repository.SoftDeleteAsync(admin);

        await _actionLogger.LogDeleteAsync(adminId, "Administrator", id,
            $"Deleted administrator {adminEmail}");

        return true;
    }
}
