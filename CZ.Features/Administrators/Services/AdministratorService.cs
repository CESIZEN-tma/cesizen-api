using api.CZ.Features.Administrators.DTOs;
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

        return admins.Select(a => new GetAdministratorDto
        {
            Id = a.Id,
            Email = a.Email,
            FirstName = a.FirstName,
            LastName = a.LastName,
            MemberSince = a.MemberSince,
            ThumbnailUrl = a.ThumbnailUrl,
            AccountActivated = a.AccountActivated,
            CreationTime = a.CreationTime,
            UpdateTime = a.UpdateTime
        });
    }

    public async Task<GetAdministratorDto?> GetByIdAsync(Guid id)
    {
        var admin = await _repository.FindAsync(id);

        if (admin == null || admin.DeletionTime != null)
            return null;

        return new GetAdministratorDto
        {
            Id = admin.Id,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            MemberSince = admin.MemberSince,
            ThumbnailUrl = admin.ThumbnailUrl,
            AccountActivated = admin.AccountActivated,
            CreationTime = admin.CreationTime,
            UpdateTime = admin.UpdateTime
        };
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
            $"Created administrator account for {admin.Email}");

        return new GetAdministratorDto
        {
            Id = admin.Id,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            MemberSince = admin.MemberSince,
            ThumbnailUrl = admin.ThumbnailUrl,
            AccountActivated = admin.AccountActivated,
            CreationTime = admin.CreationTime,
            UpdateTime = admin.UpdateTime
        };
    }

    public async Task<GetAdministratorDto?> UpdateAsync(Guid id, UpdateAdministratorDto dto, Guid adminId)
    {
        var admin = await _repository.FindAsync(id);

        if (admin == null || admin.DeletionTime != null)
            return null;

        admin.FirstName = dto.FirstName;
        admin.LastName = dto.LastName;
        admin.ThumbnailUrl = dto.ThumbnailUrl;
        admin.UpdateTime = DateTime.UtcNow;

        await _repository.UpdateAsync(admin);

        await _actionLogger.LogUpdateAsync(adminId, "Administrator", id,
            $"Updated administrator profile for {admin.Email}");

        return new GetAdministratorDto
        {
            Id = admin.Id,
            Email = admin.Email,
            FirstName = admin.FirstName,
            LastName = admin.LastName,
            MemberSince = admin.MemberSince,
            ThumbnailUrl = admin.ThumbnailUrl,
            AccountActivated = admin.AccountActivated,
            CreationTime = admin.CreationTime,
            UpdateTime = admin.UpdateTime
        };
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
