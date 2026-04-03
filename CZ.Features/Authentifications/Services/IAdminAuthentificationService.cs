using api.CZ.Core.ResultPattern;
using api.CZ.Features.Authentifications.DTOs;
using Simply.Auth.AspNetCore.Models;

namespace api.CZ.Features.Authentifications.Services;

public interface IAdminAuthentificationService
{
    Task<Result<SimplyAuthResponse>> Login(LoginDto dto);
    Task<Result> ConfirmAccount(string token);
    Task<Result> RegisterAdmin(RegisterDto dto);
    Task<Result> ForgotPassword(ForgotPasswordDto dto);
    Task<Result> ResetPassword(ResetPasswordDto dto);
    Task<Result<SimplyAuthResponse>> RefreshToken(RefreshTokenDto dto);
    Task<Result> Logout(string refreshToken);
    Task<Result<List<SessionDto>>> GetActiveSessions(Guid adminId, string currentRefreshToken);
    Task<Result> RevokeSession(Guid adminId, Guid sessionId);
    Task<Result> RevokeAllOtherSessions(Guid adminId, string currentRefreshToken);
    Task<Result> ChangePassword(Guid adminId, ChangePasswordDto dto, string currentRefreshToken);
}
