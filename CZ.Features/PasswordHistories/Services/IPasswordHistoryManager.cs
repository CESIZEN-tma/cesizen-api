namespace api.CZ.Features.PasswordHistories.Services;

public interface IPasswordHistoryManager
{
    Task<bool> IsPasswordReusedAsync(Guid userId, string newPassword);
    Task AddPasswordToHistoryAsync(Guid userId, string passwordHash);
    Task EnsurePasswordsInfoExistsAsync(Guid userId);
}
